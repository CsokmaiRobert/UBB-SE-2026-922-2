using System;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public Guid CurrentUserId { get; set; }
    }

    internal sealed class FakeSessionContext : ISessionContext
    {
        public Guid AccountId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLoggedIn { get; set; }
        public int PopulateCallCount { get; private set; }
        public int ClearCallCount { get; private set; }

        public void Populate(AccountProfileDataTransferObject profile)
        {
            this.PopulateCallCount++;
            this.AccountId = profile.Id;
            this.Username = profile.Username;
            this.DisplayName = profile.DisplayName;
            this.Email = profile.Email;
            this.PhoneNumber = profile.PhoneNumber;
            this.Country = profile.Country;
            this.City = profile.City;
            this.StreetName = profile.StreetName;
            this.StreetNumber = profile.StreetNumber;
            this.Role = profile.Role.Name;
            this.IsLoggedIn = true;
        }

        public void Clear()
        {
            this.ClearCallCount++;
            this.AccountId = Guid.Empty;
            this.Username = string.Empty;
            this.DisplayName = string.Empty;
            this.Email = string.Empty;
            this.PhoneNumber = string.Empty;
            this.Country = string.Empty;
            this.City = string.Empty;
            this.StreetName = string.Empty;
            this.StreetNumber = string.Empty;
            this.Role = string.Empty;
            this.IsLoggedIn = false;
        }
    }
}
