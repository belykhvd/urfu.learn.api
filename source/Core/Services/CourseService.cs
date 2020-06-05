using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Task;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Services
{
    public class CourseService : Repo<Course>, ICourseService
    {
        private readonly ITaskService taskService;
        private readonly FileRepo fileRepo;

        public CourseService(IConfiguration config, ITaskService taskService, FileRepo fileRepo) 
             : base(config, PgSchema.course)
        {
            this.taskService = taskService;
            this.fileRepo = fileRepo;
        }

        public async Task<IEnumerable<CourseIndex>> SelectIndexes()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<CourseIndex>(
                $@"select jsonb_build_object('id', id, 'name', name, 'maxScore', max_score) from {PgSchema.course_index}").ConfigureAwait(false);
        }

        public async Task<IEnumerable<TaskProgress>> GetProgress(Guid courseId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<TaskProgress>(
                $@"select jsonb_build_object('id', ti.id, 'name', ti.name, 'maxScore', ti.max_score, 'requirements', ti.requirements, 'currentScore', coalesce(tp.score, 0), 'done', tp.done)
                       from {PgSchema.course_tasks} ct
                       join {PgSchema.task_index} ti
                         on ct.task_id = ti.id
                       left join {PgSchema.task_progress} tp
                         on tp.user_id = @UserId
                        and ti.id = tp.task_id
                       where ct.course_id = @CourseId", new {courseId, userId}).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Link>> SelectLinks()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<Link>(
                @$"select jsonb_build_object('id', id, 'text', name) from {PgSchema.course_index}").ConfigureAwait(false);
        }

        public async Task<Guid> AddTask(Guid courseId, CourseTask task)
        {
            task.Id = Guid.NewGuid();

            var taskId = await taskService.Save(task).ConfigureAwait(false);

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.course_tasks} (course_id, task_id)
                       values (@CourseId, @TaskId)", new {courseId, taskId}).ConfigureAwait(false);

            return taskId;
        }

        public async Task DeleteTask(Guid courseId, Guid taskId)
        {
            await taskService.Delete(taskId).ConfigureAwait(false);

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"delete from {PgSchema.course_tasks}
                         where course_id = @CourseId
                           and task_id = @TaskId", new {courseId, taskId}).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Link>> SelectTasks(Guid courseId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<Link>(
                @$"select jsonb_build_object('id', ti.id, 'text', ti.name)
                       from {PgSchema.course_tasks} ct
                       join {PgSchema.task_index} ti
                         on ct.task_id = ti.id
                       where ct.course_id = @CourseId", new {courseId}).ConfigureAwait(false);
        }

        protected override async Task SaveIndex(NpgsqlConnection conn, Guid id, Course data)
        {
            await conn.ExecuteAsync(
                @$"insert into {PgSchema.course_index} (id, name)
                       values (@Id, @Name)
                       on conflict (id) do update set name = @Name", new {id, data.Name}).ConfigureAwait(false);
        }

        protected override async Task DeleteIndex(NpgsqlConnection conn, Guid id)
        {
            await conn.ExecuteAsync(@$"delete from {PgSchema.course_index} where id = @Id", new {id}).ConfigureAwait(false);
        }
    }
}