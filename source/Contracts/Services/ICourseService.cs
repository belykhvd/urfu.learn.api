using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Challenge;
using Contracts.Types.Common;
using Contracts.Types.Course;

namespace Contracts.Services
{
    public interface ICourseService : ICrudRepo<Course>
    {
        Task<IEnumerable<CourseDescription>> SelectCourses();
        Task<IEnumerable<CourseDescription>> SelectEnrolledCourses(Guid userId);
        Task<IEnumerable<ChallengeDescription>> SelectChallenges(Guid courseId);
        Task<Result> Enroll(Guid courseId);
        Task Leave(Guid courseId);
    }
}