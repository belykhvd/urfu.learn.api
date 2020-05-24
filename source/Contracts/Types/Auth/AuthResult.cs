using System;
using Contracts.Types.User;

namespace Contracts.Types.Auth
{
    public class AuthResult
    {
        public Guid UserId { get; set; }
        public Role Role { get; set; }
        public string Fio { get; set; }
    }
}