using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class TaskService : Repo<CourseTask>, ITaskService
    {
        private readonly FileRepo fileRepo;

        public TaskService(IConfiguration config, FileRepo fileRepo) : base(config, PgSchema.task)
        {
            this.fileRepo = fileRepo;
        }

        public async Task<Attachment> GetInputLink(Guid taskId)
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

        public async Task<Attachment> GetSolutionLink(Guid taskId, Guid authorId)
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
    }
}