using System;
using Contracts.Types.Course;
using Contracts.Types.CourseTask;
using Contracts.Types.Media;

namespace Contracts.Types.Task
{
    public class CourseTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DescriptionText { get; set; }
        public DateTime? Deadline { get; set; }
        
        public int? CurrentScore { get; set; }
        public int? MaxScore { get; set; }
        public RequirementStatus[] RequirementList { get; set; }

        public Attachment Input { get; set; }

        public ChallengeStatus Status { get; set; }
    }
}