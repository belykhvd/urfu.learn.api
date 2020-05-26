using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Task;
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

        [HttpGet]
        [Route("download")]
        public async Task<FileResult> DownloadInputData(Guid taskId)
        {
            var content = await taskService.DownloadInputData(taskId).ConfigureAwait(false);
            return File(content, "application/octet-stream", $"{taskId}");
        }
    }
}