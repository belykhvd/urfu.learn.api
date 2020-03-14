using Contracts.Services;
using Contracts.Types.Challenge;
using Core.Repo;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class ChallengeService : CrudRepo<Challenge>, IChallengeService
    {
        public ChallengeService(IConfiguration config) : base(config, PgSchema.Challenge)
        {
        }
    }
}