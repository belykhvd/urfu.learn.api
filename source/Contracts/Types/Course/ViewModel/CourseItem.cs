using System;

namespace Contracts.Types.Course.ViewModel
{
    public class CourseItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DescriptionText { get; set; }
        public int? CurrentScore { get; set; }
        public int? MaxScore { get; set; }
        public bool CourseStatus { get; set; }
        public TaskItem[] CourseTasks { get; set; }
    }
}