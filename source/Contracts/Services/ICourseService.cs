using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.CourseTask;
using Contracts.Types.Common;
using Contracts.Types.Course;

namespace Contracts.Services
{
    public interface ICourseService : IRepo<Course>
    {
        Task<IEnumerable<Link>> Select();
        Task<Guid> AddTask(Guid courseId, CourseTask task);
        Task DeleteTask(Guid courseId, Guid taskId);
        Task<IEnumerable<Link>> SelectTasks(Guid courseId);
    }
}