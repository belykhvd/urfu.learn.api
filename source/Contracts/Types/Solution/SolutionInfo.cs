using System;

namespace Contracts.Types.Solution
{
    public class SolutionInfo
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}