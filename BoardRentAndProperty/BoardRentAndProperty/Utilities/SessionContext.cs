namespace BoardRentAndProperty.Utilities
{
    using System;
    using BoardRentAndProperty.Models;

    public class SessionContext : ISessionContext
    {
        private const int UnauthenticatedPamUserId = 0;

        public Guid AccountId { get; private set; }

        public string Username { get; private set; }

        public string DisplayName { get; private set; }

        public string Role { get; private set; }

        public int PamUserId { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public string PhoneNumber { get; private set; }
        public string Email { get; private set; }
        public string Country { get; private set; }
        public string City { get; private set; }
        public string StreetName { get; private set; }
        public string StreetNumber { get; private set; }

        public void Populate(Account account, string roleName)
        {
            if (account != null)
            {
                this.AccountId = account.Id;
                this.Username = account.Username;
                this.DisplayName = account.DisplayName;
                this.Role = roleName;
                this.PamUserId = account.PamUserId ?? UnauthenticatedPamUserId;
                this.IsLoggedIn = true;

                this.PhoneNumber = account.PhoneNumber;
                this.City = account.City;
                this.Country = account.Country;
                this.StreetName = account.StreetName;
                this.StreetNumber = account.StreetNumber;
            }
        }

        public void Clear()
        {
            this.AccountId = Guid.Empty;
            this.Username = string.Empty;
            this.DisplayName = string.Empty;
            this.Role = string.Empty;
            this.PamUserId = UnauthenticatedPamUserId;
            this.IsLoggedIn = false;
            this.Email = string.Empty;
            this.PhoneNumber = string.Empty;
            this.City = string.Empty;
            this.Country = string.Empty;
            this.StreetName = string.Empty;
            this.StreetNumber = string.Empty;
        }
    }
}
