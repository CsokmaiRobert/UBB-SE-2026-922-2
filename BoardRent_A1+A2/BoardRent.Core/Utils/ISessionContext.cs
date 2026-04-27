namespace BoardRent.Utils
{
    using System;
    using BoardRent.Domain;

    public interface ISessionContext
    {
        Guid UserId { get; }

        string Username { get; }

        string DisplayName { get; }

        string Role { get; }

        bool IsLoggedIn { get; }

        void Populate(User user, string roleName);

        void Clear();
    }
}