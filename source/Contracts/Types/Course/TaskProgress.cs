using System;

namespace Contracts.Types.Course
{
    public class TaskProgress
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public RequirementStatus[] Progress { get; set; }
    }
}