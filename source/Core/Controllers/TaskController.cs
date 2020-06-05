﻿using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using Repo;

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
        [Authorize]
        public async Task<Guid> Save([FromBody] CourseTask course)
            => await taskService.Save(course).ConfigureAwait(false);

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<CourseTask>> Get([FromQuery] Guid taskId, [FromQuery] Guid? userId)
        {
            var task =  await taskService.Get(taskId, userId).ConfigureAwait(false);
            if (task == null)
                return NotFound();

            return task;
        }

        [HttpGet]
        [Authorize]
        public async Task<Attachment> GetSolutionLink([FromQuery] Guid taskId, [FromQuery] Guid userId)
        {
            return await taskService.GetSolutionLink(taskId, userId).ConfigureAwait(true);
        }

        [HttpGet]
        [Authorize]
        public async Task<Attachment> GetInputLink([FromQuery] Guid taskId)
        {
            return await taskService.GetInputLink(taskId).ConfigureAwait(true);
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(Constants._2GB)]
        [RequestFormLimits(MultipartBodyLengthLimit = Constants._2GB)]
        public async Task<ActionResult<Guid>> UploadSolution([FromQuery] Guid taskId, [FromForm] IFormFile file)
        {
            if (HttpContext.User.Identity.Name == null || !Guid.TryParse(HttpContext.User.Identity.Name, out var authorId))
                return Unauthorized();

            var filename = WebUtility.HtmlEncode(file.FileName);
            var attachmentId = await fileRepo.SaveAttachment(file, filename, authorId);

            await taskService.RegisterAttachment(taskId, authorId, attachmentId, AttachmentType.Solution).ConfigureAwait(true);

            return attachmentId;
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(Constants._2GB)]
        [RequestFormLimits(MultipartBodyLengthLimit = Constants._2GB)]
        public async Task<ActionResult<Guid>> UploadInput([FromQuery] Guid taskId, [FromForm] IFormFile file)
        {
            if (HttpContext.User.Identity.Name == null || !Guid.TryParse(HttpContext.User.Identity.Name, out var authorId))
                return Unauthorized();

            var filename = WebUtility.HtmlEncode(file.FileName);
            var attachmentId = await fileRepo.SaveAttachment(file, filename, authorId);

            await taskService.RegisterAttachment(taskId, authorId, attachmentId, AttachmentType.Input).ConfigureAwait(true);

            return attachmentId;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadAttachment([FromQuery] Guid attachmentId)
        {
            var attachment = await fileRepo.GetAttachment(attachmentId).ConfigureAwait(true);
            var stream = fileRepo.StreamFile(attachmentId);

            return File(stream, "application/octet-stream", attachment.Name);
        }

        [HttpGet]
        [Authorize]
        public async Task<TestResults> GetTestResults([FromQuery] Guid solutionId)
            => await taskService.GetTestResults(solutionId).ConfigureAwait(true);
    }
}