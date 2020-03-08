using System;
using Contracts.Types.Common;

namespace Contracts.Types.Course
{
    public class Course : DbEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? FinishDateTime { get; set; }
        public DateTime? EnrollStartDateTime { get; set; }
        public DateTime? EnrollFinishDateTime { get; set; }
    }
}