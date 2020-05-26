using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface ITaskService : IRepo<CourseTask>
    {
        new Task<Guid> Save(CourseTask task);

        Task<byte[]> DownloadInputData(Guid taskId);

        //Task UpdateTaskProgress(Guid userId, Guid taskId, Guid[] done);
    }
}