using System;

namespace Contracts.Types.Solution
{
    public class Solution
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }

        public Guid TaskId { get; set; }
        public Guid StudentId { get; set; }

        public string ContentBase64 { get; set; }
    }
}