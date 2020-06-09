using System;
using System.ComponentModel.DataAnnotations;

namespace Contracts.Types.Group
{
    public class Group
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string OfficialName { get; set; }

        [Required]
        public int Year { get; set; }

        public int Semester { get; set; }
    }
}