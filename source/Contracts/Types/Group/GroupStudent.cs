using System;

namespace Contracts.Types.Group
{
    public class GroupStudent
    {
        public Guid StudentId { get; set; }
        public string Email { get; set; }
        public bool IsActivated { get; set; } // принял инвайт в группу
    }
}