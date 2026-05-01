using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;

        public AdminService(IAccountRepository accountRepository, IFailedLoginRepository failedLoginRepository)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
        }

        public async Task<ServiceResult<List<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int pageNumber, int pageSize)
        {
            List<Account> accountEntities = await this.accountRepository.GetAllAsync(pageNumber, pageSize);

            List<AccountProfileDataTransferObject> accountProfileDtos = new List<AccountProfileDataTransferObject>();

            foreach (Account accountEntity in accountEntities)
            {
                Role? firstRole = accountEntity.Roles?.FirstOrDefault();
                FailedLoginAttempt? failedAttempt = await this.failedLoginRepository.GetByAccountIdAsync(accountEntity.Id);

                bool isLocked = failedAttempt != null
                    && failedAttempt.LockedUntil.HasValue
                    && failedAttempt.LockedUntil.Value > DateTime.UtcNow;

                accountProfileDtos.Add(new AccountProfileDataTransferObject
                {
                    Id = accountEntity.Id,
                    Username = accountEntity.Username,
                    DisplayName = accountEntity.DisplayName,
                    Email = accountEntity.Email,
                    PhoneNumber = accountEntity.PhoneNumber,
                    AvatarUrl = accountEntity.AvatarUrl,
                    Role = new RoleDataTransferObject
                    {
                        Id = firstRole?.Id ?? Guid.Empty,
                        Name = firstRole?.Name ?? "Standard User",
                    },
                    IsSuspended = accountEntity.IsSuspended,
                    IsLocked = isLocked,
                    Country = accountEntity.Country,
                    City = accountEntity.City,
                    StreetName = accountEntity.StreetName,
                    StreetNumber = accountEntity.StreetNumber,
                });
            }

            return ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accountProfileDtos);
        }

        public async Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId)
        {
            Account? accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            accountEntity.IsSuspended = true;
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId)
        {
            Account? accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            accountEntity.IsSuspended = false;
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword)
        {
            const int MinimumPasswordLength = 6;
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < MinimumPasswordLength)
            {
                return ServiceResult<bool>.Fail("Password must be at least 6 characters long.");
            }

            Account? accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            accountEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId)
        {
            await this.failedLoginRepository.ResetAsync(accountId);
            return ServiceResult<bool>.Ok(true);
        }
    }
}
