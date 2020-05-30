using System;
using System.ComponentModel.DataAnnotations;

namespace Contracts.Types.Group
{
    public class Group
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string OfficialName { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Semester { get; set; }
    }
}