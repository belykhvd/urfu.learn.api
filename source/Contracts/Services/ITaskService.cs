using Contracts.Repo;
using Contracts.Types.CourseTask;

namespace Contracts.Services
{
    public interface ITaskService : IRepo<CourseTask>
    {
    }
}