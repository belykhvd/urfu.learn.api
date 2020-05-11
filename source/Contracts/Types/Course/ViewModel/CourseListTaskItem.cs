using System;

namespace Contracts.Types.Course.ViewModel
{
    public class CourseListTaskItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RequirementStatus[] RequirementStatusList { get; set; } 
    }
}