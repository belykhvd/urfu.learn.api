using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface ITaskService : IRepo<CourseTask>
    {
        //Task UpdateTaskProgress(Guid userId, Guid taskId, Guid[] done);
    }
}