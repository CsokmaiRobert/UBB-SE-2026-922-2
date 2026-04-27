namespace BoardRentAndProperty.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;

    public class AuthService : IAuthService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;

        public AuthService(
            IAccountRepository accountRepository,
            IFailedLoginRepository failedLoginRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
            ISessionContext sessionContext)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);

                Account existingAccountByUsername = await this.accountRepository.GetByUsernameAsync(registrationRequest.Username);
                if (existingAccountByUsername != null)
                {
                    return ServiceResult<bool>.Fail("Username|Username is already taken.");
                }

                Account newAccount = new Account
                {
                    Id = Guid.NewGuid(),
                    DisplayName = registrationRequest.DisplayName,
                    Username = registrationRequest.Username,
                    Email = registrationRequest.Email,
                    PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSuspended = false
                };

                await this.accountRepository.AddAsync(newAccount);
                await this.accountRepository.AddRoleAsync(newAccount.Id, "Standard User");

                this.sessionContext.Populate(newAccount, "Standard User");
                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject loginRequest)
        {
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.accountRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                Account accountEntity = await this.accountRepository.GetByUsernameAsync(loginRequest.UsernameOrEmail);
                if (accountEntity == null)
                {
                    accountEntity = await this.accountRepository.GetByEmailAsync(loginRequest.UsernameOrEmail);
                }

                if (accountEntity == null)
                {
                    return ServiceResult<AccountProfileDataTransferObject>.Fail("Invalid username or password.");
                }

                if (accountEntity.IsSuspended)
                {
                    return ServiceResult<AccountProfileDataTransferObject>.Fail("This account has been suspended.");
                }

                if (!PasswordHasher.VerifyPassword(loginRequest.Password, accountEntity.PasswordHash))
                {
                    await this.failedLoginRepository.IncrementAsync(accountEntity.Id);
                    return ServiceResult<AccountProfileDataTransferObject>.Fail("Invalid username or password.");
                }

                await this.failedLoginRepository.ResetAsync(accountEntity.Id);

                string primaryRole = accountEntity.Roles?.FirstOrDefault()?.Name ?? "Standard User";
                this.sessionContext.Populate(accountEntity, primaryRole);

                AccountProfileDataTransferObject profileDto = new AccountProfileDataTransferObject
                {
                    Id = accountEntity.Id,
                    Username = accountEntity.Username,
                    DisplayName = accountEntity.DisplayName,
                    Email = accountEntity.Email,
                    Role = new RoleDataTransferObject { Name = primaryRole }
                };

                return ServiceResult<AccountProfileDataTransferObject>.Ok(profileDto);
            }
        }

        public async Task<ServiceResult<bool>> LogoutAsync()
        {
            this.sessionContext.Clear();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync()
        {
            return ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com.");
        }
    }
}
