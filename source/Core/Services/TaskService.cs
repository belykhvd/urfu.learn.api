using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Task;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class TaskService : Repo<CourseTask>, ITaskService
    {
        private readonly MediaRepo mediaRepo;

        public TaskService(IConfiguration config, MediaRepo mediaRepo) : base(config, PgSchema.task)
        {
            this.mediaRepo = mediaRepo;
        }

        public async Task<Guid> Save(CourseTask task)
        {
            throw new NotImplementedException();
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

        public async Task<byte[]> DownloadInputData(Guid taskId)
        {



            return await mediaRepo.ReadFile(taskId).ConfigureAwait(false);
        }
    }
}