using System;
using System.IO;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Media;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface ITaskService : IRepo<CourseTask>
    {
        new Task<Guid> Save(CourseTask task);
        
        Task<Attachment> GetSolutionLink(Guid taskId, Guid userId);
        Task RegisterSolution(Guid taskId, Guid authorId, Guid attachmentId);
        Task<FileStream> DownloadSolution(Guid attachmentId);
        Task<FileStream> DownloadInputData(Guid taskId);
        
    }
}