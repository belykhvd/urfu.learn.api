using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Types.Challenge;
using Contracts.Types.Course;
using Contracts.Types.CourseTask;
using Microsoft.AspNetCore.Mvc;

namespace Core.IControllers
{
    public interface ICourseController
    {
        Task<IEnumerable<CourseDescription>> SelectCourses();
        Task<IActionResult> Add(Course course);
        Task<IActionResult> Delete(Guid courseId);
        Task<IActionResult> Get(Guid courseId);
        Task<IEnumerable<ChallengeDescription>> SelectChallenges(Guid courseId);
        Task<IActionResult> Enroll(Guid courseId);
        Task<IActionResult> Leave(Guid courseId);
        Task<IActionResult> Update(Guid courseId, Course course);

        Task<IActionResult> AddChallenge(Challenge challenge);
        Task<IActionResult> GetChallenge(Guid challengeId);
    }
}