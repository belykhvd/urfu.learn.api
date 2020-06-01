using System;

namespace Contracts.Types.Course
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MaxScore { get; set; }
        public DateTime? Deadline { get; set; }
    }
}