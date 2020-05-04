using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class CourseService : Repo<Course>, ICourseService
    {
        public CourseService(IConfiguration config) : base(config, PgSchema.course) {}

        public async Task<IEnumerable<Link>> Select()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<Link>(
                @$"select jsonb_build_object('id', id, 'text', name) from {PgSchema.course_index}").ConfigureAwait(false);
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