using System;

namespace Contracts.Types.Course.ViewModel
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RequirementStatus[] RequirementStatusList { get; set; }
        public int? CurrentScore { get; set; }
        public int? MaxScore { get; set; }
        public Guid? SolutionId { get; set; }
    }
}