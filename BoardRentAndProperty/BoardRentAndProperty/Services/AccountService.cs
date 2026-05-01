namespace BoardRentAndProperty.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Mappers;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;

    public class AccountService : IAccountService
    {
        private const int MinimumDisplayNameLength = 2;
        private const int MaximumDisplayNameLength = 50;
        private const int MaximumStreetNumberLength = 10;
        private const string AvatarFolderName = "Avatars";
        private const string ApplicationName = "BoardRent";

        private readonly IAccountRepository accountRepository;
        private readonly ISessionContext sessionContext;
        private readonly AccountProfileMapper accountProfileMapper;

        public AccountService(IAccountRepository accountRepository, ISessionContext sessionContext, AccountProfileMapper accountProfileMapper)
        {
            this.accountRepository = accountRepository;
            this.sessionContext = sessionContext;
            this.accountProfileMapper = accountProfileMapper;
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail("Account not found.");
            }

            return ServiceResult<AccountProfileDataTransferObject>.Ok(this.accountProfileMapper.ToDataTransferObject(accountEntity));
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData)
        {
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

            this.accountProfileMapper.ApplyToEntity(accountEntity, profileUpdateData);
            await this.accountRepository.UpdateAsync(accountEntity);
            RefreshSessionContext(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
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

        public async Task<string> UploadAvatarAsync(Guid accountId, string sourceFilePath)
        {
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
            RefreshSessionContext(accountEntity);

            return destinationPath;
        }

        public async Task RemoveAvatarAsync(Guid accountId)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                throw new InvalidOperationException("Account not found.");
            }

            accountEntity.AvatarUrl = null;
            accountEntity.UpdatedAt = DateTime.UtcNow;

            await this.accountRepository.UpdateAsync(accountEntity);
            RefreshSessionContext(accountEntity);
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

        private void RefreshSessionContext(Account accountEntity)
        {
            if (this.sessionContext.AccountId == accountEntity.Id && this.sessionContext.IsLoggedIn)
            {
                this.sessionContext.Populate(accountEntity, this.sessionContext.Role);
            }
        }
    }
}
