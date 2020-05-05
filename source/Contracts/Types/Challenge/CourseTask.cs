using System;
using Contracts.Types.Solution;

namespace Contracts.Types.CourseTask
{
    public class CourseTask
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public ChallengeStatus Status { get; set; }
        public Guid AttachmentId { get; set; }
        public Requirement[] Requirements { get; set; }
        public SolutionInfo[] Solutions { get; set; }
    }
}