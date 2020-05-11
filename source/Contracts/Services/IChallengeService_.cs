using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.CourseTask;
using Contracts.Types.Common;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface IChallengeService
    {
        Task<Result<CourseTask>> Get(Guid challengeId, Guid userId);
    }
}