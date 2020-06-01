using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Course;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

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

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Route("saveMultipart")]
        [RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        [RequestSizeLimit(268435456)]
        public async Task<ActionResult<Guid>> SaveWithAttachment()
        {
            const int MultipartBoundaryLengthLimit = 70;
            const int ValueCountLimit = 10;
            
            var attachment = new Attachment();
            
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest("IsMultipartContentType");
            }

            // Accumulate the form data key-value pairs in the request (formAccumulator).
            var formAccumulator = new KeyValueAccumulator();

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), MultipartBoundaryLengthLimit);
            
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync(); 
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                { 
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        // Don't trust the file name sent by the client.
                        // To display the file name, HTML-encode the value.
                        attachment.Name = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
                        
                        attachment.Id = await FileHelpers.WriteStreamOnDisk(section, contentDisposition).ConfigureAwait(true);
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Don't limit the key name length because the multipart headers length limit is already in effect. 
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                        var encoding = GetEncoding(section);

                        if (encoding == null)
                        {
                            return BadRequest("encoding == null");
                        }

                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        { 
                            // The value length limit is enforced by MultipartBodyLengthLimit 
                            var value = await streamReader.ReadToEndAsync();

                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }

                            formAccumulator.Append(key, value);

                            if (formAccumulator.ValueCount > ValueCountLimit)
                            {
                                return BadRequest("key count limit exceeded");
                            }
                        }    
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            attachment.Timestamp = DateTime.UtcNow;
            
            var task = new CourseTask
            {
                Name = "",
                DescriptionText = "",
                Deadline = DateTime.Now,
                RequirementList = new RequirementStatus[0]
            };
            
            return Ok(attachment.Id);
        }
        
        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }
}