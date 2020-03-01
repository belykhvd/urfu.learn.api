using System;

namespace Contracts.Types.Solution
{
    public class SolutionProgress
    {
        // SERVER GEN | CLIENT<-
        public DateTime Timestamp { get; set; }

        // MENTOR-> | CLIENT<-
        public Guid MentorId { get; set; }
        
        // MENTOR-> | CLIENT<-
        public Guid SolutionId { get; set; }
        
        // MENTOR-> | CLIENT<-
        public double Progress { get; set; }

        // CLIENT<-
        public string MentorName { get; set; }
    }
}