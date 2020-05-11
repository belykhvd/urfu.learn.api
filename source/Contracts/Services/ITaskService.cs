using Contracts.Repo;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface ITaskService : IRepo<CourseTask>
    {
    }
}