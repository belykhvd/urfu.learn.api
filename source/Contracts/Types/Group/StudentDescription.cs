using System;

namespace Contracts.Types.Group
{
    public class StudentDescription
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public Guid PhotoId { get; set; }
    }
}