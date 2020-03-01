using System;
using Contracts.Types.Solution;

namespace Contracts.Types.CourseTask
{
    public class CourseTask
    {
        // To client, gen on server
        public Guid Id { get; set; }

        // DB, To client, From client
        public string Title { get; set; }
        public string Description { get; set; }

        // To client
        public SolutionStudentSummary[] SolutionSummaryBatch { get; set; } 
    }
}