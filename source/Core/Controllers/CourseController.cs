using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.CourseTask;
using Contracts.Types.Common;
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

        [HttpGet]
        [Route("select")]
        public async Task<IEnumerable<Link>> Select()
            => await courseService.Select().ConfigureAwait(false);

        [HttpPost]
        [Route("save")]
        public async Task<Guid> Save([FromQuery] Guid? id, [FromBody] Course course)
            => await courseService.Save(id, course).ConfigureAwait(false);

        [HttpGet]
        [Route("get")]
        public async Task<Course> Get([FromQuery] Guid id)
            => await courseService.Get(id).ConfigureAwait(false);

        [HttpPost]
        [Route("delete")]
        public async Task Delete([FromQuery] Guid id)
            => await courseService.Delete(id).ConfigureAwait(false);

        [HttpPost]
        [Route("addTask")]
        public async Task<Guid> AddTask([FromQuery] Guid courseId, [FromBody] CourseTask task)
            => await courseService.AddTask(courseId, task).ConfigureAwait(false);

        [HttpPost]
        [Route("deleteTask")]
        public async Task DeleteTask([FromQuery] Guid courseId, [FromQuery] Guid taskId)
            => await courseService.DeleteTask(courseId, taskId).ConfigureAwait(false);

        [HttpGet]
        [Route("selectTasks")]
        public async Task<IEnumerable<Link>> SelectTasks([FromQuery] Guid courseId)
            => await courseService.SelectTasks(courseId).ConfigureAwait(false);
    }
}