using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Media;
using Contracts.Types.Task;

namespace Contracts.Services
{
    public interface ICourseService : IRepo<Course>
    {
        Task<IEnumerable<CourseIndex>> SelectIndexes();
        Task<IEnumerable<TaskProgress>> GetProgress(Guid courseId, Guid userId);

        Task<IEnumerable<Link>> SelectLinks();

        Task<Guid> AddTask(Guid courseId, CourseTask task);
        Task DeleteTask(Guid courseId, Guid taskId);
        Task<IEnumerable<Link>> SelectTasks(Guid courseId);
    }
}