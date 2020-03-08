using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.Course;

namespace Contracts.Services
{
    public interface ICourseService : ICrudRepo<Course>
    {
        Task SelectEnrolledCourses(Guid userId, SelectRange range);
    }
}