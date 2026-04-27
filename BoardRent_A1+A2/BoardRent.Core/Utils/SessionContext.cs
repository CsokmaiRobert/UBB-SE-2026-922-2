namespace BoardRent.Utils
{
    using System;
    using BoardRent.Domain;

    public class SessionContext : ISessionContext
    {
        public Guid UserId { get; private set; }

        public string Username { get; private set; }

        public string DisplayName { get; private set; }

        public string Role { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public void Populate(User user, string roleName)
        {
            if (user != null)
            {
                this.UserId = user.Id;
                this.Username = user.Username;
                this.DisplayName = user.DisplayName;
                this.Role = roleName;
                this.IsLoggedIn = true;
            }
        }

        public void Clear()
        {
            this.UserId = Guid.Empty;
            this.Username = string.Empty;
            this.DisplayName = string.Empty;
            this.Role = string.Empty;
            this.IsLoggedIn = false;
        }
    }
}