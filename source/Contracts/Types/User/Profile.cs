﻿namespace Contracts.Types.User
{
    public class Profile
    {
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Group { get; set; }

        public string Fio => $"{Surname} {FirstName} {SecondName}";
    }
}