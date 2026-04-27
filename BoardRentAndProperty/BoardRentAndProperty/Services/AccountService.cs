namespace BoardRentAndProperty.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;

    public class AccountService : IAccountService
    {
        private const int MinimumDisplayNameLength = 2;
        private const int MaximumDisplayNameLength = 50;
        private const int MaximumStreetNumberLength = 10;
        private const string StandardAccountRoleName = "Standard User";
        private const string AvatarFolderName = "Avatars";
        private const string ApplicationName = "BoardRent";

        private readonly IAccountRepository accountRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;

        public AccountService(IAccountRepository accountRepository, IUnitOfWorkFactory unitOfWorkFactory, ISessionContext sessionContext)
        {
            this.accountRepository = accountRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId)
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<AccountProfileDataTransferObject>.Fail("Account not found.");
                }

                return ServiceResult<AccountProfileDataTransferObject>.Ok(this.MapEntityToProfileDataTransferObject(accountEntity));
            }
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData)
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                var validationErrors = this.ValidateProfileDetails(profileUpdateData);

                if (!string.IsNullOrWhiteSpace(profileUpdateData.Email) && profileUpdateData.Email != accountEntity.Email)
                {
                    var accountWithDuplicateEmail = await this.accountRepository.GetByEmailAsync(profileUpdateData.Email);
                    if (accountWithDuplicateEmail != null && accountWithDuplicateEmail.Id != accountId)
                    {
                        validationErrors.Add("Email|This email address is already taken by another account.");
                    }
                }

                if (validationErrors.Any())
                {
                    return ServiceResult<bool>.Fail(string.Join(";", validationErrors));
                }

                this.ApplyProfileUpdatesToEntity(accountEntity, profileUpdateData);
                await this.accountRepository.UpdateAsync(accountEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                if (!PasswordHasher.VerifyPassword(currentPassword, accountEntity.PasswordHash))
                {
                    return ServiceResult<bool>.Fail("Current password is incorrect.");
                }

                var (isPasswordValid, passwordErrorMessage) = PasswordValidator.Validate(newPassword);
                if (!isPasswordValid)
                {
                    return ServiceResult<bool>.Fail(passwordErrorMessage);
                }

                accountEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
                accountEntity.UpdatedAt = DateTime.UtcNow;

                await this.accountRepository.UpdateAsync(accountEntity);
                this.sessionContext.Clear();

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<string> UploadAvatarAsync(Guid accountId, string sourceFilePath)
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    throw new InvalidOperationException("Account not found.");
                }

                string fileName = $"{accountId}_{Path.GetFileName(sourceFilePath)}";
                string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string saveFolderPath = Path.Combine(localApplicationData, ApplicationName, AvatarFolderName);

                Directory.CreateDirectory(saveFolderPath);
                string destinationPath = Path.Combine(saveFolderPath, fileName);

                File.Copy(sourceFilePath, destinationPath, true);

                accountEntity.AvatarUrl = destinationPath;
                accountEntity.UpdatedAt = DateTime.UtcNow;

                await this.accountRepository.UpdateAsync(accountEntity);

                return destinationPath;
            }
        }

        public async Task RemoveAvatarAsync(Guid accountId)
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    throw new InvalidOperationException("Account not found.");
                }

                accountEntity.AvatarUrl = null;
                accountEntity.UpdatedAt = DateTime.UtcNow;

                await this.accountRepository.UpdateAsync(accountEntity);
            }
        }

        private List<string> ValidateProfileDetails(AccountProfileDataTransferObject profileData)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(profileData.DisplayName) ||
                profileData.DisplayName.Length < MinimumDisplayNameLength ||
                profileData.DisplayName.Length > MaximumDisplayNameLength)
            {
                errors.Add("DisplayName|Display name must be between 2 and 50 characters long.");
            }

            if (!string.IsNullOrWhiteSpace(profileData.PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(profileData.PhoneNumber, @"^\+?\d{7,15}$"))
                {
                    errors.Add("PhoneNumber|Phone number format is invalid.");
                }
            }

            if (!string.IsNullOrWhiteSpace(profileData.StreetNumber) && profileData.StreetNumber.Length > MaximumStreetNumberLength)
            {
                errors.Add("StreetNumber|Street number must be a valid value.");
            }

            return errors;
        }

        private void ApplyProfileUpdatesToEntity(Account accountEntity, AccountProfileDataTransferObject profileUpdateData)
        {
            accountEntity.DisplayName = profileUpdateData.DisplayName;
            accountEntity.Email = profileUpdateData.Email;
            accountEntity.PhoneNumber = profileUpdateData.PhoneNumber;
            accountEntity.Country = profileUpdateData.Country;
            accountEntity.City = profileUpdateData.City;
            accountEntity.StreetName = profileUpdateData.StreetName;
            accountEntity.StreetNumber = profileUpdateData.StreetNumber;
            accountEntity.UpdatedAt = DateTime.UtcNow;
        }

        private AccountProfileDataTransferObject MapEntityToProfileDataTransferObject(Account accountEntity)
        {
            var primaryRole = accountEntity.Roles?.FirstOrDefault();

            return new AccountProfileDataTransferObject
            {
                Id = accountEntity.Id,
                Username = accountEntity.Username,
                DisplayName = accountEntity.DisplayName,
                Email = accountEntity.Email,
                PhoneNumber = accountEntity.PhoneNumber,
                AvatarUrl = accountEntity.AvatarUrl,
                Role = new RoleDataTransferObject
                {
                    Id = primaryRole?.Id ?? Guid.Empty,
                    Name = primaryRole?.Name ?? StandardAccountRoleName,
                },
                IsSuspended = accountEntity.IsSuspended,
                Country = accountEntity.Country,
                City = accountEntity.City,
                StreetName = accountEntity.StreetName,
                StreetNumber = accountEntity.StreetNumber,
            };
        }
    }
}
