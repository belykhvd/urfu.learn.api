using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Core.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TaskController : FileControllerBase
    {
        private readonly ITaskService taskService;

        public TaskController(ITaskService taskService, FileRepo fileRepo) : base(fileRepo)
        {
            this.taskService = taskService;
        }

        [HttpPost]
        public async Task<Guid> Save([FromQuery] Guid id, [FromBody] CourseTask course)
            => await taskService.Save(id, course).ConfigureAwait(false);

        [HttpGet]
        public async Task<CourseTask> Get([FromQuery] Guid id)
            => await taskService.Get(id).ConfigureAwait(false);

        [HttpGet]
        [Route("download")]
        public async Task<FileResult> DownloadInputData(Guid taskId)
        {
            var content = await taskService.DownloadInputData(taskId).ConfigureAwait(false);
            return File(content, "application/octet-stream", $"{taskId}");
        }

        [HttpGet]
        [Authorize]
        public async Task<Attachment> GetSolutionLink([FromQuery] Guid taskId, [FromQuery] Guid userId)
        {
            return await taskService.GetSolutionLink(taskId, userId).ConfigureAwait(true);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Guid>> UploadSolution([FromQuery] Guid taskId)
        {
            if (HttpContext.User.Identity.Name == null || !Guid.TryParse(HttpContext.User.Identity.Name, out var authorId))
                return Unauthorized();
            
            var attachmentId = await UploadFile().ConfigureAwait(true);
            if (attachmentId == null)
                return BadRequest(Constants.SolutionUploadError);

            await taskService.RegisterSolution(taskId, authorId, attachmentId.Value).ConfigureAwait(true);

            return attachmentId.Value;
        }
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<FileStreamResult>> DownloadSolution([FromQuery] Guid solutionId)
        {
            var attachment = await fileRepo.GetAttachment(solutionId).ConfigureAwait(true);
            var stream = fileRepo.StreamFile(solutionId);
                
            return File(stream, "application/octet-stream", $"{taskId}");
        }
        
        [HttpGet]
        [Authorize]
        public async Task<Action<FileStreamResult> 
    }
}