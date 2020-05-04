using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Types.Solution;

namespace Contracts.Services
{
    public interface ISolutionService
    {
        Task<IEnumerable<SolutionInfo>> GetSolutionList(Guid userId, Guid challengeId);

        // STUDENT
        Task Upload(Solution solution);

        // STUDENT + MENTOR
        Task<byte[]> Download(Guid solutionId);

        // STUDENT
        Task<IEnumerable<SolutionInfo>> SelectStudentSummaries(Guid taskId, int lastLoadedIndex, int limit);
        
        // MENTOR
        Task RateProgress(SolutionProgress progress);
    }
}