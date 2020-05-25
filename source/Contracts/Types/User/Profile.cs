using System.Text.Json.Serialization;

namespace Contracts.Types.User
{
    public class Profile
    {
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Group { get; set; }

        [JsonIgnore]
        public string Fio => $"{Surname} {FirstName?.Substring(0, 1)}. {SecondName?.Substring(0, 1)}.";
    }
}