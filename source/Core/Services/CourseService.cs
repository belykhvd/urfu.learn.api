using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Core.Repo;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class CourseService : CrudRepo<Course>, ICourseService
    {
        public CourseService(IConfiguration config) : base(config, "course")
        {
        }

        public Task SelectEnrolledCourses(Guid userId, SelectRange range)
        {
            throw new NotImplementedException();
        }
    }
}