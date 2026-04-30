namespace BoardRentAndProperty.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Constants;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Mappers;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;
        private readonly AccountProfileMapper accountProfileMapper;

        public AccountService(IAccountRepository accountRepository, IUnitOfWorkFactory unitOfWorkFactory, ISessionContext sessionContext, AccountProfileMapper accountProfileMapper)
        {
            this.accountRepository = accountRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
            this.accountProfileMapper = accountProfileMapper;
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId)
        {
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<AccountProfileDataTransferObject>.Fail("Account not found.");
                }

                return ServiceResult<AccountProfileDataTransferObject>.Ok(this.accountProfileMapper.ToDataTransferObject(accountEntity));
            }
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData)
        {
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                List<string> validationErrors = AccountProfileValidator.Validate(profileUpdateData);

                if (!string.IsNullOrWhiteSpace(profileUpdateData.Email) && profileUpdateData.Email != accountEntity.Email)
                {
                    Account accountWithDuplicateEmail = await this.accountRepository.GetByEmailAsync(profileUpdateData.Email);
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

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
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
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    throw new InvalidOperationException("Account not found.");
                }

                string fileName = $"{accountId}_{Path.GetFileName(sourceFilePath)}";
                string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string saveFolderPath = Path.Combine(localApplicationData, DomainConstants.ApplicationName, DomainConstants.AvatarFolderName);

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
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    throw new InvalidOperationException("Account not found.");
                }

                accountEntity.AvatarUrl = null;
                accountEntity.UpdatedAt = DateTime.UtcNow;

                await this.accountRepository.UpdateAsync(accountEntity);
            }
        }
    }
}