using System;

namespace Contracts.Types.Challenge
{
    public class Challenge
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public Requirement[] Requirements { get; set; }
    }
}