namespace BoardRentAndProperty.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;

    public class AdminService : IAdminService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;
        private readonly INotificationService notificationService;

        public AdminService(
            IAccountRepository accountRepository,
            IFailedLoginRepository failedLoginRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
            ISessionContext sessionContext,
            INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
            this.notificationService = notificationService;
        }

        private bool IsAuthorized()
        {
            return this.sessionContext.IsLoggedIn && this.sessionContext.Role == "Administrator";
        }

        public async Task<ServiceResult<List<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int pageNumber, int pageSize)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<List<AccountProfileDataTransferObject>>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                List<Account> accountEntities = await this.accountRepository.GetAllAsync(1, int.MaxValue);

                List<AccountProfileDataTransferObject> accountProfileDtos = new List<AccountProfileDataTransferObject>();
                foreach (Account accountEntity in accountEntities)
                {
                    Role firstRole = accountEntity.Roles?.FirstOrDefault();
                    FailedLoginAttempt failedAttempt = await this.failedLoginRepository.GetByAccountIdAsync(accountEntity.Id);

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
                            Name = firstRole?.Name ?? "Standard User"
                        },
                        IsSuspended = accountEntity.IsSuspended,
                        IsLocked = isLocked,
                        Country = accountEntity.Country,
                        City = accountEntity.City,
                        StreetName = accountEntity.StreetName,
                        StreetNumber = accountEntity.StreetNumber
                    });
                }

                return ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accountProfileDtos);
            }
        }

        public async Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                accountEntity.IsSuspended = true;
                await this.accountRepository.UpdateAsync(accountEntity);

                this.NotifyUser(accountEntity, "Your account has been suspended by an administrator.");

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                accountEntity.IsSuspended = false;
                await this.accountRepository.UpdateAsync(accountEntity);

                this.NotifyUser(accountEntity, "Your account has been unsuspended by an administrator.");

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            var validationResult = PasswordValidator.Validate(newPassword);
            if (!validationResult.IsValid)
            {
                return ServiceResult<bool>.Fail(validationResult.Error);
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                accountEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
                await this.accountRepository.UpdateAsync(accountEntity);

                this.NotifyUser(accountEntity, "Your password has been reset by an administrator.");

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByIdAsync(accountId);
                if (accountEntity == null)
                {
                    return ServiceResult<bool>.Fail("Account not found.");
                }

                await this.failedLoginRepository.ResetAsync(accountId);

                this.NotifyUser(accountEntity, "Your account has been unlocked by an administrator.");

                return ServiceResult<bool>.Ok(true);
            }
        }

        private void NotifyUser(Account account, string message)
        {
            if (account.PamUserId.HasValue)
            {
                var notification = new NotificationDTO
                {
                    Title = "System Notification",
                    Body = message,
                    Timestamp = DateTime.UtcNow,
                    Type = NotificationType.Informational
                };

                this.notificationService.SendNotificationToUser(account.PamUserId.Value, notification);
            }
        }
    }
}