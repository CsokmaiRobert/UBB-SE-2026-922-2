namespace BoardRentAndProperty.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;

    public class AuthService : IAuthService
    {
        private const string StandardUserRoleName = "Standard User";

        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;
        private readonly IUserRepository pamUserRepository;

        public AuthService(
            IAccountRepository accountRepository,
            IFailedLoginRepository failedLoginRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
            ISessionContext sessionContext,
            IUserRepository pamUserRepository)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
            this.pamUserRepository = pamUserRepository;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            if (string.IsNullOrWhiteSpace(registrationRequest.Username) ||
                string.IsNullOrWhiteSpace(registrationRequest.Email) ||
                string.IsNullOrWhiteSpace(registrationRequest.DisplayName) ||
                string.IsNullOrWhiteSpace(registrationRequest.Password))
            {
                return ServiceResult<bool>.Fail("Form|All fields are required.");
            }

            if (registrationRequest.Password != registrationRequest.ConfirmPassword)
            {
                return ServiceResult<bool>.Fail("Password|Passwords do not match.");
            }

            try
            {
                int linkedPamUserId = this.CreatePamUserForDisplayName(registrationRequest.DisplayName);

                Account newAccount;

                using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
                {
                    await unitOfWork.OpenAsync();
                    this.accountRepository.SetUnitOfWork(unitOfWork);

                    if (await this.accountRepository.GetByUsernameAsync(registrationRequest.Username) != null)
                    {
                        return ServiceResult<bool>.Fail("Username|Username is already taken.");
                    }

                    if (await this.accountRepository.GetByEmailAsync(registrationRequest.Email) != null)
                    {
                        return ServiceResult<bool>.Fail("Email|Email is already registered.");
                    }

                    newAccount = new Account
                    {
                        Id = Guid.NewGuid(),
                        DisplayName = registrationRequest.DisplayName,
                        Username = registrationRequest.Username,
                        Email = registrationRequest.Email,
                        PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsSuspended = false,
                        PamUserId = linkedPamUserId
                    };

                    await this.accountRepository.AddAsync(newAccount);
                    await this.accountRepository.AddRoleAsync(newAccount.Id, StandardUserRoleName);
                }

                this.sessionContext.Populate(newAccount, StandardUserRoleName);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Database Error|{ex.Message}");
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

                if (accountEntity.PamUserId == null)
                {
                    int lazilyCreatedPamUserId = this.CreatePamUserForDisplayName(accountEntity.DisplayName);
                    await this.accountRepository.SetPamUserIdAsync(accountEntity.Id, lazilyCreatedPamUserId);
                    accountEntity.PamUserId = lazilyCreatedPamUserId;
                }

                string primaryRole = accountEntity.Roles?.FirstOrDefault()?.Name ?? StandardUserRoleName;
                this.sessionContext.Populate(accountEntity, primaryRole);

                AccountProfileDataTransferObject profileDto = new AccountProfileDataTransferObject
                {
                    Id = accountEntity.Id,
                    Username = accountEntity.Username,
                    DisplayName = accountEntity.DisplayName,
                    Email = accountEntity.Email,

                    PhoneNumber = accountEntity.PhoneNumber,
                    Country = accountEntity.Country,
                    City = accountEntity.City,
                    StreetName = accountEntity.StreetName,
                    StreetNumber = accountEntity.StreetNumber,
                    AvatarUrl = accountEntity.AvatarUrl,

                    Role = new RoleDataTransferObject { Name = primaryRole },
                };

                return ServiceResult<AccountProfileDataTransferObject>.Ok(profileDto);
            }
        }

        public Task<ServiceResult<bool>> LogoutAsync()
        {
            this.sessionContext.Clear();
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }

        public Task<ServiceResult<string>> ForgotPasswordAsync()
        {
            return Task.FromResult(ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com."));
        }

        private int CreatePamUserForDisplayName(string displayName)
        {
            User pamUserToInsert = new User
            {
                DisplayName = displayName,
            };

            this.pamUserRepository.Add(pamUserToInsert);
            return pamUserToInsert.Id;
        }
    }
}
