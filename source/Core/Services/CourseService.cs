using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.CourseTask;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class CourseService : CrudRepo<Course>, ICourseService
    {
        public CourseService(IConfiguration config) : base(config, PgSchema.Course) {}

        public async Task<IEnumerable<CourseDescription>> SelectCourses()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<CourseDescription>(
                @$"select id, name, short_description from {PgSchema.CourseIndex}").ConfigureAwait(false);
        }

        public Task<IEnumerable<CourseDescription>> SelectEnrolledCourses(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChallengeDescription>> SelectChallenges(Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task<Result> Enroll(Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task Leave(Guid courseId)
        {
            throw new NotImplementedException();
        }
    }
}