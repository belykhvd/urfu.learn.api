using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts.Types.Challenge
{
    public class ChallengeBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public RequirementBase[] Requirements { get; set; }

        public Challenge ToChallenge(HashSet<int> accomplishedRequirements) => new Challenge
        {
            Name = Name,
            Description = Description,
            Deadline = Deadline,
            Requirements = Requirements.Select(x => new Requirement
            {
                Number = x.Number,
                Name = x.Name,
                IsAccomplished = accomplishedRequirements.Contains(x.Number)
            }).ToArray()
        };
    }
}