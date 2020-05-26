namespace Contracts.Types.Auth
{
    public class AuthData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }
}