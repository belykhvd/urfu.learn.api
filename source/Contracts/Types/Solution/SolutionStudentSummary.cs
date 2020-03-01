using System;

namespace Contracts.Types.Solution
{
    // ONLY TO CLIENT
    public class SolutionStudentSummary
    {
        public Guid SolutionId { get; set; }
        public DateTime Timestamp { get; set; }

        public string FileName { get; set; }
        public long FileSize { get; set; }
        
        public string Comment { get; set; }

        public SolutionProgress Progress { get; set; }
    }
}