using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.CourseTask;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService taskService;

        public TaskController(ITaskService taskService)
        {
            this.taskService = taskService;
        }

        [HttpPost]
        [Route("save")]
        public async Task<Guid> Save([FromQuery] Guid id, [FromBody] CourseTask course)
            => await taskService.Save(id, course).ConfigureAwait(false);

        [HttpGet]
        [Route("get")]
        public async Task<CourseTask> Get([FromQuery] Guid id)
            => await taskService.Get(id).ConfigureAwait(false);
    }
}