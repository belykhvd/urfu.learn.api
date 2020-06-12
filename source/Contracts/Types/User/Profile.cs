using Contracts.Types.Auth;

namespace Contracts.Types.User
{
    public class Profile
    {
        public string Email { get; set; }

        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Group { get; set; }

        public UserRole Role { get; set; } 

        public string Fio() => $"{Surname} {FirstName} {SecondName}"; 
        public string Initials() => $"{Surname} {FirstName?.Substring(0, 1)}. {SecondName?.Substring(0, 1)}" + (SecondName?.Length > 0 ? "." : "");
    }
}