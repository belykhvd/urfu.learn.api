using System;

namespace Contracts.Types.Auth
{
    public class AuthResult
    {
        public Guid UserId { get; set; }
        public UserRole Role { get; set; }
        public string Fio { get; set; }
    }
}