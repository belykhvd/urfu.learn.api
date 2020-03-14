using System;

namespace Contracts.Types.Course
{
    public class CourseDescription
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
    }
}