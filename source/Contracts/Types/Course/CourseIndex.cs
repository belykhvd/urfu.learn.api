using System;

namespace Contracts.Types.Course
{
    public class CourseIndex
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MaxScore { get; set; }
    }
}