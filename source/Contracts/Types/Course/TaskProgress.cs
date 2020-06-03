using System;
using Contracts.Types.Task;

namespace Contracts.Types.Course
{
    public class TaskProgress
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? CurrentScore { get; set; }
        public int? MaxScore { get; set; }
        public Requirement[] Requirements { get; set; }
        public Guid[] Done { get; set; }
    }
}