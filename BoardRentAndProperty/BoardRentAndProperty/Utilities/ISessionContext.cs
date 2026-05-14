namespace BoardRentAndProperty.Utilities
{
    using System;
    using BoardRentAndProperty.Contracts.DataTransferObjects;

    public interface ISessionContext
    {
        Guid AccountId { get; }
        string Username { get; }
        string DisplayName { get; }
        string Email { get; }
        string PhoneNumber { get; }
        string Country { get; }
        string City { get; }
        string StreetName { get; }
        string StreetNumber { get; }
        string Role { get; }
        bool IsLoggedIn { get; }
        void Populate(AccountProfileDataTransferObject profile);
        void Clear();
    }
}
