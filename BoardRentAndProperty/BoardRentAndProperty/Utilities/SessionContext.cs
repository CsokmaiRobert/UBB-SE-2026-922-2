namespace BoardRentAndProperty.Utilities
{
    using System;
    using BoardRentAndProperty.Contracts.DataTransferObjects;

    public class SessionContext : ISessionContext
    {
        private const string StandardUserRoleName = "Standard User";

        public Guid AccountId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string StreetName { get; private set; } = string.Empty;
        public string StreetNumber { get; private set; } = string.Empty;
        public string Role { get; private set; } = string.Empty;
        public bool IsLoggedIn { get; private set; }

        public void Populate(AccountProfileDataTransferObject profile)
        {
            if (profile == null)
            {
                return;
            }

            AccountId = profile.Id;
            Username = profile.Username ?? string.Empty;
            DisplayName = profile.DisplayName ?? string.Empty;
            Email = profile.Email ?? string.Empty;
            PhoneNumber = profile.PhoneNumber ?? string.Empty;
            Country = profile.Country ?? string.Empty;
            City = profile.City ?? string.Empty;
            StreetName = profile.StreetName ?? string.Empty;
            StreetNumber = profile.StreetNumber ?? string.Empty;
            Role = profile.Role?.Name ?? StandardUserRoleName;
            IsLoggedIn = true;
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
