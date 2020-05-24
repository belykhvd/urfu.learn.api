namespace Contracts.Types.User
{
    public class RegistrationData
    {
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string Group { get; set; }

        public Role Role { get; set; }
    }
}