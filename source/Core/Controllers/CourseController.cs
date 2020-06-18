using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Course.ViewModel;
using Contracts.Types.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]/[action]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService courseService;
        private readonly ITaskService taskService;

        public CourseController(ICourseService courseService, ITaskService taskService)
        {
            this.courseService = courseService;
            this.taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseItem>>> Select([FromQuery] Guid? userId)
        {
            if (!Guid.TryParse(HttpContext.User.Identity.Name, out var senderId))
                return Unauthorized();

            var isAdmin = HttpContext.User.IsInRole(nameof(UserRole.Admin));
            if (!isAdmin)
                userId = senderId;

            var items = new List<CourseItem>();

            var courses = await courseService.Select(userId).ConfigureAwait(false);
            foreach (var course in courses)
            {
                var courseItem = new CourseItem
                {
                    Id = course.Id,
                    Name = course.Name,
                    MaxScore = course.MaxScore,
                    DescriptionText = course.DescriptionText
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

                    if (userId != null)
                    {
                        var solutionAttachment = await taskService.GetSolutionAttachment(taskItem.Id, userId.Value).ConfigureAwait(false);
                        if (solutionAttachment != null && solutionAttachment.Id != Guid.Empty)
                            taskItem.SolutionId = solutionAttachment.Id;    
                    }

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

                courseItem.CourseTasks = taskItems.OrderBy(x => x.Name).ToArray();
                courseItem.CurrentScore = taskItems.Sum(x => x.CurrentScore);

                items.Add(courseItem);
            }

            return items.OrderBy(x => x.Name).ToArray();
        }

        [HttpGet]
        public async Task<IEnumerable<Link>> SelectLinks()
            => await courseService.SelectLinks().ConfigureAwait(false);

        [HttpPost]
        [Authorize(Roles = Constants.ProfessorOrAdmin)]
        public async Task<Guid> Save([FromQuery] Guid? id, [FromBody] Course course)
        {
            if (id != null)
                course.Id = id.Value;

            return await courseService.Save(course).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<ActionResult<Course>> Get([FromQuery] Guid id)
        {
            var course = await courseService.Get(id).ConfigureAwait(false);
            if (course == null)
                return NotFound();

            return course;
        }
            

        [HttpPost]
        [Authorize(Roles = Constants.ProfessorOrAdmin)]
        public async Task Delete([FromQuery] Guid id)
            => await courseService.Delete(id).ConfigureAwait(false);

        [HttpPost]
        [Authorize(Roles = Constants.ProfessorOrAdmin)]
        public async Task<Guid> AddTask([FromQuery] Guid courseId, [FromBody][Required] CourseTask task)
            => await courseService.AddTask(courseId, task).ConfigureAwait(false);

        [HttpPost]
        [Authorize(Roles = Constants.ProfessorOrAdmin)]
        public async Task DeleteTask([FromQuery] Guid courseId, [FromQuery] Guid taskId)
            => await courseService.DeleteTask(courseId, taskId).ConfigureAwait(false);

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<Link>> SelectTasks([FromQuery] Guid courseId)
            => await courseService.SelectTasks(courseId).ConfigureAwait(false);
    }
}