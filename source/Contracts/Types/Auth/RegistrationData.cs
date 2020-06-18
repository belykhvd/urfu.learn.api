using System.ComponentModel.DataAnnotations;
using Contracts.Types.User;

namespace Contracts.Types.Auth
{
    public class RegistrationData
    {
        [Required]
        public string Surname { get; set; }

        [Required]
        public string FirstName { get; set; }
        public string SecondName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Group { get; set; }

        public UserRole Role { get; set; }

        public Profile Profile => new Profile
        {
            Email = Email.Trim(),
            Surname = Surname.Trim(),
            FirstName = FirstName.Trim(),
            SecondName = SecondName?.Trim(),
            Group = Group?.Trim()
        };

        public AuthData AuthData => new AuthData
        {
            Email = Email.Trim(),
            Password = Password.Trim(),
            Role = Role
        };
    }
}