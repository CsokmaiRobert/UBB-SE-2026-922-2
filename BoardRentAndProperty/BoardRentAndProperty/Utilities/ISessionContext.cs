namespace BoardRentAndProperty.Utilities
{
    using System;
    using BoardRentAndProperty.Models;

    public interface ISessionContext
    {
        Guid AccountId { get; }

        string Username { get; }

        string DisplayName { get; }

        string Role { get; }

        bool IsLoggedIn { get; }

        void Populate(Account account, string roleName);

        void Clear();
    }
}
