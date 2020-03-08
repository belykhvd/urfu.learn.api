using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Course;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService courseService;

        public CourseController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        #region CRUD

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add([FromBody] Course course)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // TODO authorization
            // TODO preprocessing and validation

            return (await courseService.Create(course).ConfigureAwait(false)).ActionResult();
        }

        [HttpGet]
        [Route("get/{courseId}")]
        public async Task<IActionResult> Get(Guid courseId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await courseService.Read(courseId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] Course course)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await courseService.Update(course).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromQuery] Guid courseId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await courseService.Delete(courseId).ConfigureAwait(false)).ActionResult();
        }

        #endregion
    }
}