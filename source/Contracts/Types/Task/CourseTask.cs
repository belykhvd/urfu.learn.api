using System;
using Contracts.Types.Course;
using Contracts.Types.CourseTask;
using Contracts.Types.Media;
using Contracts.Types.Solution;

namespace Contracts.Types.Task
{
    public class CourseTask
    {
        public string Name { get; set; }
        public string DescriptionText { get; set; }
        public DateTime? Deadline { get; set; }
        public int? CurrentScore { get; set; }
        public int? MaxScore { get; set; }
        public RequirementStatus[] RequirementList { get; set; }

        public Attachment InputData { get; set; }

        public ChallengeStatus Status { get; set; }
        public SolutionInfo[] Solutions { get; set; }
    }
}