using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Course.ViewModel;
using Contracts.Types.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repo;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService courseService;
        private readonly ITaskService taskService;
        private readonly FileRepo fileRepo;

        public CourseController(ICourseService courseService, ITaskService taskService)
        {
            this.courseService = courseService;
            this.taskService = taskService;
        }

        [HttpGet]
        public async Task<IEnumerable<CourseItem>> Select([FromQuery] Guid userId)
        {
            var items = new List<CourseItem>();

            var courses = await courseService.SelectIndexes().ConfigureAwait(false);

            foreach (var course in courses)
            {
                var courseItem = new CourseItem
                {
                    Id = course.Id,
                    Name = course.Name,
                    MaxScore = course.MaxScore
                };

                var taskItems = new List<TaskItem>();

                var taskProgressRecords = await courseService.GetProgress(course.Id, userId).ConfigureAwait(false);
                foreach (var taskProgress in taskProgressRecords)
                {
                    var taskItem = new TaskItem
                    {
                        Id = taskProgress.Id,
                        Name = taskProgress.Name,
                        MaxScore = taskProgress.MaxScore,
                        CurrentScore = taskProgress.CurrentScore,
                        RequirementStatusList = taskProgress.Requirements?.Select(r => new RequirementStatus
                        {
                            Id = r.Id,
                            Text = r.Text
                        }).ToArray()
                    };

                    var doneRequirements = taskProgress.Done?.ToHashSet();

                    if (taskItem.RequirementStatusList != null && doneRequirements != null)
                    {
                        foreach (var requirement in taskItem.RequirementStatusList)
                        {
                            if (doneRequirements.Contains(requirement.Id))
                                requirement.Status = true;
                        }
                    }

                    taskItems.Add(taskItem);
                }

                courseItem.CourseTasks = taskItems.ToArray();
                courseItem.CurrentScore = taskItems.Sum(x => x.CurrentScore);

                items.Add(courseItem);
            }

            return items;
        }

        [HttpGet]
        public async Task<IEnumerable<Link>> SelectLinks()
            => await courseService.SelectLinks().ConfigureAwait(false);

        [HttpPost]
        [Authorize]
        public async Task<Guid> Save([FromQuery] Guid? id, [FromBody] Course course)
            => await courseService.Save(id, course).ConfigureAwait(false);

        [HttpGet]
        public async Task<Course> Get([FromQuery] Guid id)
            => await courseService.Get(id).ConfigureAwait(false);

        [HttpPost]
        [Authorize]
        public async Task Delete([FromQuery] Guid id)
            => await courseService.Delete(id).ConfigureAwait(false);

        [HttpPost]
        [Authorize]
        public async Task<Guid> AddTask([FromQuery] Guid courseId, [FromBody] CourseTask task)
            => await courseService.AddTask(courseId, task).ConfigureAwait(false);

        // [HttpPost]
        // [Authorize]
        // public async Task<ActionResult<Guid>> AddTask([FromQuery] Guid courseId)
        // {
        //     var (keyValue, attachment, error) = await UploadMultipartForm().ConfigureAwait(true);
        //     if (error != null)
        //         return BadRequest(error);
        //
        //     if (!keyValue.TryGetValue("task", out var stringValues) || stringValues.Count != 1)
        //     {
        //         return BadRequest("Отсутствует поле task или stringValues.Count != 1");
        //     }
        //
        //     var serializedTask = stringValues[0];
        //     var task = JsonConvert.DeserializeObject<CourseTask>(serializedTask);
        //
        //     await courseService.AddTask(courseId, task).ConfigureAwait(false);
        // }


        [HttpPost]
        [Authorize]
        public async Task DeleteTask([FromQuery] Guid courseId, [FromQuery] Guid taskId)
            => await courseService.DeleteTask(courseId, taskId).ConfigureAwait(false);

        [HttpGet]
        public async Task<IEnumerable<Link>> SelectTasks([FromQuery] Guid courseId)
            => await courseService.SelectTasks(courseId).ConfigureAwait(false);
        
        
        
        
        
        // [HttpPost]
        // //[ValidateAntiForgeryToken]
        // [Route("saveMultipart")]
        // [RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        // [RequestSizeLimit(268435456)]
        // public async Task<ActionResult<Guid>> SaveWithAttachment()
        // {
        //     const int MultipartBoundaryLengthLimit = 70;
        //     const int ValueCountLimit = 2;
        //     
        //     var attachment = new Attachment();
        //     
        //     if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        //         return BadRequest("IsMultipartContentType");
        //
        //     // Accumulate the form data key-value pairs in the request (formAccumulator).
        //     var formAccumulator = new KeyValueAccumulator();
        //
        //     var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), MultipartBoundaryLengthLimit);
        //     
        //     var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        //     var section = await reader.ReadNextSectionAsync(); 
        //     while (section != null)
        //     {
        //         var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
        //         if (hasContentDispositionHeader)
        //         { 
        //             if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
        //             {
        //                 // Don't trust the file name sent by the client.
        //                 // To display the file name, HTML-encode the value.
        //                 attachment.Name = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
        //                 attachment.Id = await FileHelpers.WriteStreamOnDisk(section, contentDisposition).ConfigureAwait(true);
        //             }
        //             else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
        //             {
        //                 // Don't limit the key name length because the multipart headers length limit is already in effect. 
        //                 var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
        //                 var encoding = GetEncoding(section);
        //
        //                 if (encoding == null)
        //                     return BadRequest("encoding == null");
        //
        //                 using (var streamReader = new StreamReader(section.Body, encoding,
        //                     detectEncodingFromByteOrderMarks: true,
        //                     bufferSize: 1024,
        //                     leaveOpen: true))
        //                 { 
        //                     // The value length limit is enforced by MultipartBodyLengthLimit 
        //                     var value = await streamReader.ReadToEndAsync();
        //
        //                     if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
        //                     {
        //                         value = string.Empty;
        //                     }
        //
        //                     formAccumulator.Append(key, value);
        //
        //                     if (formAccumulator.ValueCount > ValueCountLimit)
        //                     {
        //                         return BadRequest("key count limit exceeded");
        //                     }
        //                 }    
        //             }
        //         }
        //
        //         // Drain any remaining section body that hasn't been consumed and
        //         // read the headers for the next section.
        //         section = await reader.ReadNextSectionAsync();
        //     }
        //
        //     attachment.Timestamp = DateTime.UtcNow;
        //
        //
        //     
        //     
        //     return Ok(attachment.Id);
        // }
        //
        // private const int MultipartBoundaryLengthLimit = 70;
        // private const int ValueCountLimit = 2;
        //
        // private async Task<(IReadOnlyDictionary<string, StringValues>, Attachment attachment, string errorMessage)> UploadMultipartForm()
        // {
        //     // Accumulate the form data key-value pairs in the request (formAccumulator).
        //     var formAccumulator = new KeyValueAccumulator();
        //     var attachment = new Attachment();
        //     
        //     if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        //         return (null, null, "IsMultipartContentType");
        //     
        //     var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), MultipartBoundaryLengthLimit);
        //     var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        //     var section = await reader.ReadNextSectionAsync(); 
        //     while (section != null)
        //     {
        //         var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
        //         if (hasContentDispositionHeader)
        //         { 
        //             if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
        //             {
        //                 // Don't trust the file name sent by the client. To display the file name, HTML-encode the value.
        //                 var nameHtmlEncoded = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
        //                 
        //                 await fileRepo.SaveAttachment()
        //                 
        //                 attachment.Id = await fileRepo.WriteOnDisk(section).ConfigureAwait(true);
        //                 attachment.Timestamp = DateTime.UtcNow;
        //                 attachment.Size = fileRepo.GetFileSize(attachment.Id);
        //             }
        //             else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
        //             {
        //                 // Don't limit the key name length because the multipart headers length limit is already in effect. 
        //                 var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
        //                 var encoding = GetEncoding(section);
        //
        //                 if (encoding == null)
        //                     return (null, null, "encoding == null");
        //
        //                 using (var streamReader = new StreamReader(section.Body, encoding,
        //                     detectEncodingFromByteOrderMarks: true,
        //                     bufferSize: 1024,
        //                     leaveOpen: true))
        //                 { 
        //                     // The value length limit is enforced by MultipartBodyLengthLimit 
        //                     var value = await streamReader.ReadToEndAsync();
        //
        //                     if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
        //                     {
        //                         value = string.Empty;
        //                     }
        //
        //                     formAccumulator.Append(key, value);
        //
        //                     if (formAccumulator.ValueCount > ValueCountLimit)
        //                         return (null, null, "key count limit exceeded");
        //                 }    
        //             }
        //         }
        //
        //         // Drain any remaining section body that hasn't been consumed and
        //         // read the headers for the next section.
        //         section = await reader.ReadNextSectionAsync();
        //     }
        //     
        //     return (formAccumulator.GetResults(), attachment, null);
        // }
        //
        //
        // private static Encoding GetEncoding(MultipartSection section)
        // {
        //     var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);
        //
        //     // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in most cases.
        //     if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        //     {
        //         return Encoding.UTF8;
        //     }
        //
        //     return mediaType.Encoding;
        // }
    }
}