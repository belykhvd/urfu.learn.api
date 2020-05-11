using System;

namespace Contracts.Types.Course.ViewModel
{
    public class CourseListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MaxScore { get; set; }
        public int CurrentScore { get; set; }
        public bool CourseStatus { get; set; }
        public CourseListTaskItem[] CourseTasks { get; set; }
    }
}