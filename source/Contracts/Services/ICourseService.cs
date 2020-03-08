using System;
using System.Threading.Tasks;
using Contracts.Types.Course;

namespace Contracts.Services
{
    public interface ICourseService
    {
        Task Create(Course course);
        
        
        

        Task SelectUserCourses(Guid userId, int? lastLoadedIndex, int limit);
    }
}