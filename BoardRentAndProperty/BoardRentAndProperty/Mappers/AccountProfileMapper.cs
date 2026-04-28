namespace BoardRentAndProperty.Mappers
{
    using System;
    using System.Linq;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Models;

    public class AccountProfileMapper
    {
        private const string StandardAccountRoleName = "Standard User";

        public AccountProfileDataTransferObject ToDto(Account account)
        {
            if (account == null)
            {
                return null;
            }

            var primaryRole = account.Roles?.FirstOrDefault();

            return new AccountProfileDataTransferObject
            {
                Id = account.Id,
                Username = account.Username,
                DisplayName = account.DisplayName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                AvatarUrl = account.AvatarUrl,
                Role = new RoleDataTransferObject
                {
                    Id = primaryRole?.Id ?? Guid.Empty,
                    Name = primaryRole?.Name ?? StandardAccountRoleName,
                },
                IsSuspended = account.IsSuspended,
                Country = account.Country,
                City = account.City,
                StreetName = account.StreetName,
                StreetNumber = account.StreetNumber,
            };
        }

        public void ApplyTo(Account account, AccountProfileDataTransferObject dto)
        {
            account.DisplayName = dto.DisplayName;
            account.Email = dto.Email;
            account.PhoneNumber = dto.PhoneNumber;
            account.Country = dto.Country;
            account.City = dto.City;
            account.StreetName = dto.StreetName;
            account.StreetNumber = dto.StreetNumber;
            account.UpdatedAt = DateTime.UtcNow;
        }
    }
}
