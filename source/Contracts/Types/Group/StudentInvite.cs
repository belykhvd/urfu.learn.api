using System;

namespace Contracts.Types.Group
{
    public class StudentInvite
    {
        public Guid? StudentId { get; set; }
        public string Email { get; set; }
        public bool IsAccepted { get; set; }
    }
}