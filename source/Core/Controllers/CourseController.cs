using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Challenge;
using Contracts.Types.Course;
using Core.IControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    public class CourseController : ControllerBase, ICourseController
    {
        private readonly ICourseService courseService;
        private readonly IChallengeService challengeService;

        public CourseController(ICourseService courseService, IChallengeService challengeService)
        {
            this.courseService = courseService;
            this.challengeService = challengeService;
        }

        [HttpGet]
        [Route("courses")]
        public async Task<IEnumerable<CourseDescription>> SelectCourses()
        {
            return await courseService.SelectCourses().ConfigureAwait(false);
        }

        [HttpPost]
        [Route("courses/add")]
        public async Task<IActionResult> Add([FromBody] Course course)
        {
            return (await courseService.Save(course).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Authorize]
        [Route("courses/delete")]
        public async Task<IActionResult> Delete([FromQuery] Guid courseId)
        {
            return (await courseService.Delete(courseId).ConfigureAwait(false)).ActionResult();
        }

        [HttpGet]
        [Route("course/{courseId}")]
        public async Task<IActionResult> Get([FromRoute] Guid courseId)
        {
            return (await courseService.Read(courseId).ConfigureAwait(false)).ActionResult();
        }

        [HttpGet]
        [Route("course/{courseId}/challenges")]
        public async Task<IEnumerable<ChallengeDescription>> SelectChallenges([FromRoute] Guid courseId)
        {
            return await courseService.SelectChallenges(courseId).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("course/{courseId}/enroll")]
        public async Task<IActionResult> Enroll([FromRoute] Guid courseId)
        {
            return Ok();
        }

        [HttpPost]
        [Route("course/{courseId}/leave")]
        public async Task<IActionResult> Leave(Guid courseId)
        {
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("course/{courseId}/update")]
        public async Task<IActionResult> Update([FromQuery] Guid courseId, [FromBody] Course course)
        {
            return (await courseService.Update(courseId, course).ConfigureAwait(false)).ActionResult();
        }
    }
}