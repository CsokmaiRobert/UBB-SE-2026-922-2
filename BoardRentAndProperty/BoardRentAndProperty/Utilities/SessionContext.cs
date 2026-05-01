namespace BoardRentAndProperty.Utilities
{
    using System;
    using BoardRentAndProperty.Models;
    public class SessionContext : ISessionContext
    {
        public Guid AccountId { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Country { get; private set; }
        public string City { get; private set; }
        public string StreetName { get; private set; }
        public string StreetNumber { get; private set; }
        public string Role { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public void Populate(Account account, string roleName)
        {
            if (account != null)
            {
                AccountId = account.Id;
                Username = account.Username;
                DisplayName = account.DisplayName;
                Email = account.Email;
                PhoneNumber = account.PhoneNumber;
                Country = account.Country;
                City = account.City;
                StreetName = account.StreetName;
                StreetNumber = account.StreetNumber;
                Role = roleName;
                IsLoggedIn = true;
            }
        }
        public void Clear()
        {
            AccountId = Guid.Empty;
            Username = string.Empty;
            DisplayName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Country = string.Empty;
            City = string.Empty;
            StreetName = string.Empty;
            StreetNumber = string.Empty;
            Role = string.Empty;
            IsLoggedIn = false;
        }
    }
}
