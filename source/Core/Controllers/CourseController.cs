using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Course.ViewModel;
using Contracts.Types.Task;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [Route("select")]
        public async Task<IEnumerable<CourseListItem>> Select([FromQuery] Guid userId)
        {
            var items = new List<CourseListItem>();

            var courses = await courseService.SelectIndexes().ConfigureAwait(false);

            foreach (var course in courses)
            {
                var courseListItem = new CourseListItem
                {
                    Id = course.Id,
                    Name = course.Name,
                    MaxScore = course.MaxScore
                };

                var taskProgressRecords = (await courseService.GetCourseProgress(course.Id, userId).ConfigureAwait(false)).ToArray();
                var taskItems = taskProgressRecords.Select(x => new CourseListTaskItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    RequirementStatusList = x.Progress
                }).ToArray();

                courseListItem.CourseTasks = taskItems;
                courseListItem.CurrentScore = taskProgressRecords.Sum(x => x.Score);

                items.Add(courseListItem);
            }

            return items;
        }

        [HttpGet]
        [Route("selectLinks")]
        public async Task<IEnumerable<Link>> SelectLinks()
            => await courseService.SelectLinks().ConfigureAwait(false);

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