namespace BoardRentAndProperty.Utilities
{
    using System;
    using BoardRentAndProperty.Models;
    public class SessionContext : ISessionContext
    {
        public Guid AccountId { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string Role { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public void Populate(Account account, string roleName)
        {
            if (account != null)
            {
                AccountId = account.Id;
                Username = account.Username;
                DisplayName = account.DisplayName;
                Role = roleName;
                IsLoggedIn = true;
            }
        }
        public void Clear()
        {
            AccountId = Guid.Empty;
            Username = string.Empty;
            DisplayName = string.Empty;
            Role = string.Empty;
            IsLoggedIn = false;
        }
    }
}
