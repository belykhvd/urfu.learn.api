using System;
using System.Threading.Tasks;
using Contracts.Types.CourseTask;
using Contracts.Types.Task;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Repo
{
    public class TaskRepo : Repo<CourseTask>
    {
        public TaskRepo(IConfiguration config, string relationName) : base(config, relationName)
        {
        }

        protected override async Task SaveIndex(NpgsqlConnection conn, Guid id, CourseTask data)
        {
            await conn.ExecuteAsync(
                @$"insert into {PgSchema.task_index} (id, name)
                       values (@Id, @Name)
                       on conflict (id) do update set name = @Name", new {id, data.Name}).ConfigureAwait(false);
        }

        protected override async Task DeleteIndex(NpgsqlConnection conn, Guid id)
        {
            await conn.ExecuteAsync(@$"delete from {PgSchema.task_index} where id = @Id", new {id}).ConfigureAwait(false);
        }
    }
}