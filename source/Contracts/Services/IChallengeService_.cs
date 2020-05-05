using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.CourseTask;
using Contracts.Types.Common;

namespace Contracts.Services
{
    public interface IChallengeService_ : IRepo<ChallengeBase>
    {
        Task<Result<CourseTask>> Get(Guid challengeId, Guid userId);
    }
}