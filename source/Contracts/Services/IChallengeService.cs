using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Challenge;
using Contracts.Types.Common;

namespace Contracts.Services
{
    public interface IChallengeService : ICrudRepo<ChallengeBase>
    {
        Task<Result<Challenge>> Get(Guid challengeId, Guid userId);
    }
}