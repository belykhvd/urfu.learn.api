using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.CourseTask;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class TaskService : Repo<CourseTask>, ITaskService
    {
        public TaskService(IConfiguration config) : base(config, PgSchema.task)
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

        //public async Task<Result<CourseTask>> Get(Guid challengeId, Guid userId)
        //{
        //    var challengeBaseResult = await Read(challengeId).ConfigureAwait(false);
        //    if (!challengeBaseResult.IsSuccess)
        //        return Result<CourseTask>.Fail(challengeBaseResult.StatusCode, challengeBaseResult.ErrorMessage);

        //    var challengeBase = challengeBaseResult.Value;

        //    await using var conn = new NpgsqlConnection(ConnectionString);
        //    var accomplishments = (await conn.QueryAsync<int>(
        //        $@"select number
        //               from {PgSchema.ChallengeAccomplishment}
        //               where user_id = @UserId
        //                 and challenge_id = @ChallengeId
        //                 order by number", new {challengeId, userId}).ConfigureAwait(false)).ToHashSet();

        //    var challenge = challengeBase.ToChallenge(accomplishments);

        //    return Result<CourseTask>.Success(challenge);
        //}
    }
}