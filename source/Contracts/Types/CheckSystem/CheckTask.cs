using System;

namespace Contracts.Types.CheckSystem
{
    public class CheckTask
    {
        public Guid TaskId { get; set; }
        public Guid SolutionId { get; set; }
    }
}