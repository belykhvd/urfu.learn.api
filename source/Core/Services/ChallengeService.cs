using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Challenge;
using Contracts.Types.Common;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class ChallengeService : Repo<ChallengeBase>, IChallengeService
    {
        public ChallengeService(IConfiguration config) : base(config, PgSchema.Challenge)
        {
        }

        public async Task<Result<Challenge>> Get(Guid challengeId, Guid userId)
        {
            var challengeBaseResult = await Read(challengeId).ConfigureAwait(false);
            if (!challengeBaseResult.IsSuccess)
                return Result<Challenge>.Fail(challengeBaseResult.StatusCode, challengeBaseResult.ErrorMessage);

            var challengeBase = challengeBaseResult.Value;

            await using var conn = new NpgsqlConnection(ConnectionString);
            var accomplishments = (await conn.QueryAsync<int>(
                $@"select number
                       from {PgSchema.ChallengeAccomplishment}
                       where user_id = @UserId
                         and challenge_id = @ChallengeId
                         order by number", new {challengeId, userId}).ConfigureAwait(false)).ToHashSet();

            var challenge = challengeBase.ToChallenge(accomplishments);

            return Result<Challenge>.Success(challenge);
        }
    }
}