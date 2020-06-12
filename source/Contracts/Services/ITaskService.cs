using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Media;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface ITaskService : IRepo<CourseTask>
    {
        new Task<Guid> Save(CourseTask task);
        Task<CourseTask> Get(Guid taskId, Guid? userId);

        Task<Attachment> GetInputAttachment(Guid taskId);
        Task<Attachment> GetSolutionAttachment(Guid taskId, Guid authorId);
        Task RegisterAttachment(Guid taskId, Guid authorId, Guid attachmentId, AttachmentType type);

        Task EnqueueSolution(Guid taskId, Guid solutionId);
        Task<TestResults> GetTestResults(Guid solutionId);
    }
}