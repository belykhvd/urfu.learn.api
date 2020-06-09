using System;
using System.ComponentModel.DataAnnotations;

namespace Contracts.Types.Course
{
    public class Course
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string DescriptionText { get; set; }
        public int? MaxScore { get; set; }
    }
}