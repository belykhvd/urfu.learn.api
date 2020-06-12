using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.CheckSystem;
using Contracts.Types.Course;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Services
{
    public class TaskService : Repo<CourseTask>, ITaskService
    {
        private readonly FileRepo fileRepo;

        public TaskService(IConfiguration config, FileRepo fileRepo) : base(config, PgSchema.task)
        {
            this.fileRepo = fileRepo;
        }

        public new async Task<Guid> Save(CourseTask task)
        {
            if (task.Id == Guid.Empty)
                task.Id = Guid.NewGuid();

            task.Input = null;

            if (task.RequirementList != null)
            {
                foreach (var requirement in task.RequirementList)
                {
                    if (requirement.Id == Guid.Empty)
                        requirement.Id = Guid.NewGuid();
                }
            }

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @$"insert into {PgSchema.task} (id, data)
                       values (@Id, @Data::jsonb)
                       on conflict (id) do update set data = @Data::jsonb", new {task.Id, Data = task}).ConfigureAwait(false);

            await SaveIndex(conn, task.Id, task).ConfigureAwait(false);

            return task.Id;
        }

        public async Task<CourseTask> Get(Guid taskId, Guid? userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var task = await conn.QuerySingleOrDefaultAsync<CourseTask>(
                $@"select data
                       from {PgSchema.task}
                       where id = @TaskId
                       limit 1", new {taskId}).ConfigureAwait(false);

            if (task == null)
                return null;

            var attachment = await GetInputAttachment(taskId).ConfigureAwait(false);
            task.Input = attachment;

            if (userId == null)
                return task;

            var progress = await GetProgress(taskId, userId.Value).ConfigureAwait(false);
            if (progress == null)
                return task;

            task.CurrentScore = progress.CurrentScore;

            var doneRequirements = progress.Done?.ToHashSet();
            foreach (var requirement in task.RequirementList)
            {
                if (doneRequirements == null)
                    break;

                if (doneRequirements.Contains(requirement.Id))
                    requirement.Status = true;
            }

            return task;
        }

        public async Task<TaskProgress> GetProgress(Guid taskId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<TaskProgress>(
                $@"select jsonb_build_object('currentScore', coalesce(tp.score, 0), 'done', tp.done)
                       from {PgSchema.task_progress} tp
                       where tp.user_id = @UserId
                         and tp.task_id = @TaskId
                       limit 1", new { userId, taskId }).ConfigureAwait(false);
        }

        public async Task<Attachment> GetInputAttachment(Guid taskId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<Attachment>(
                $@"select json_build_object(
		                    'id', fi.id,
		                    'name', fi.name,
		                    'size', fi.size,
		                    'timestamp', fi.timestamp,
		                    'author', fi.author)
	                   from {PgSchema.attachment} sol
                       left join {PgSchema.file_index} fi
                         on sol.attachment_id = fi.id
                       where sol.task_id = @TaskId
                         and sol.type = 0
                       order by sol.number desc
                       limit 1", new {taskId}).ConfigureAwait(false);
        }

        public async Task<Attachment> GetSolutionAttachment(Guid taskId, Guid authorId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<Attachment>(
                $@"select json_build_object(
                            'id', fi.id,
                            'name', fi.name,
                            'size', fi.size,
                            'timestamp', fi.timestamp,
                            'author', fi.author)
                       from {PgSchema.attachment} sol
                       left join {PgSchema.file_index} fi
                         on sol.attachment_id = fi.id
                       where sol.task_id = @TaskId
                         and sol.author_id = @AuthorId
                       order by sol.number desc
                       limit 1", new {taskId, authorId}).ConfigureAwait(false);
        }
        
        public async Task RegisterAttachment(Guid taskId, Guid authorId, Guid attachmentId, AttachmentType type)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.attachment} (task_id, author_id, attachment_id, number, type)
                       values (@TaskId, @AuthorId, @AttachmentId,
                            coalesce((select number
                                          from {PgSchema.attachment}
                                          where task_id = @TaskId
                                            and author_id = @AuthorId
                                          order by number desc
                                          limit 1), 0) + 1,
                            @Type
                       )",
                new
                {
                    taskId,
                    authorId,
                    attachmentId,
                    type
                }).ConfigureAwait(false);
        }

        public async Task EnqueueSolution(Guid taskId, Guid solutionId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.js_queue} (task_id, solution_id, status)
                       values (@TaskId, @SolutionId, @Status)",
                new
                {
                    taskId,
                    solutionId,
                    Status = CheckStatus.InQueue
                }).ConfigureAwait(false);
        }

        protected override async Task SaveIndex(NpgsqlConnection conn, Guid id, CourseTask data)
        {
            await conn.ExecuteAsync(
                @$"insert into {PgSchema.task_index} (id, name, max_score, requirements)
                       values (@Id, @Name, @MaxScore, @RequirementList::jsonb)
                       on conflict (id) do update set name = @Name,
                                                      max_score = @MaxScore,
                                                      requirements = @RequirementList::jsonb", new
                {
                    id, 
                    data.Name,
                    data.MaxScore,
                    data.RequirementList
                }).ConfigureAwait(false);
        }

        protected override async Task DeleteIndex(NpgsqlConnection conn, Guid id)
        {
            await conn.ExecuteAsync(@$"delete from {PgSchema.task_index} where id = @Id", new {id}).ConfigureAwait(false);
        }

        public async Task<TestResults> GetTestResults(Guid solutionId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<TestResults>(
                $@"select json_build_object(
                         'passed', passed,
                         'all', all_count,
                         'failedNumber', failed_number,
                         'stacktrace', stacktrace)
                       from {PgSchema.js_check_result}
                       where solution_id = @SolutionId
                       limit 1", new {solutionId}).ConfigureAwait(false);
        }
    }
}