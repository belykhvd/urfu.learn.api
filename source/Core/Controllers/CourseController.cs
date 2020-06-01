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

namespace Core.Controllers
{
    [ApiController]
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

        [HttpPost]
        [Authorize]
        public async Task DeleteTask([FromQuery] Guid courseId, [FromQuery] Guid taskId)
            => await courseService.DeleteTask(courseId, taskId).ConfigureAwait(false);

        [HttpGet]
        public async Task<IEnumerable<Link>> SelectTasks([FromQuery] Guid courseId)
            => await courseService.SelectTasks(courseId).ConfigureAwait(false);
    }
}