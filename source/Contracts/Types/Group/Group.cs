using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.Types.Group
{
    public class Group
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [JsonIgnore]
        public string OfficialName { get; set; }

        [Required]
        public int Year { get; set; }

        [JsonIgnore]
        public int Semester { get; set; }
    }
}