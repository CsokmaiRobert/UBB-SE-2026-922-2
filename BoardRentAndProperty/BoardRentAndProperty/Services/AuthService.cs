namespace BoardRentAndProperty.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.Repositories;
    using BoardRentAndProperty.Utilities;
    public class AuthService : IAuthService
    {
        private const string StandardUserRoleName = "Standard User";
        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly ISessionContext sessionContext;
        public AuthService(IAccountRepository accountRepository, IFailedLoginRepository failedLoginRepository, ISessionContext sessionContext)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.sessionContext = sessionContext;
        }
        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            var existingByUsername = await accountRepository.GetByUsernameAsync(registrationRequest.Username);
            if (existingByUsername != null)
            {
                return ServiceResult<bool>.Fail("Username|Username is already taken.");
            }
            var newAccount = new Account
            {
                Id = Guid.NewGuid(), DisplayName = registrationRequest.DisplayName, Username = registrationRequest.Username,
                Email = registrationRequest.Email, PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
                PhoneNumber = registrationRequest.PhoneNumber ?? string.Empty,
                AvatarUrl = string.Empty,
                Country = registrationRequest.Country ?? string.Empty,
                City = registrationRequest.City ?? string.Empty,
                StreetName = registrationRequest.StreetName ?? string.Empty,
                StreetNumber = registrationRequest.StreetNumber ?? string.Empty,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsSuspended = false
            };
            await accountRepository.AddAsync(newAccount);
            await accountRepository.AddRoleAsync(newAccount.Id, StandardUserRoleName);
            sessionContext.Populate(newAccount, StandardUserRoleName);
            return ServiceResult<bool>.Ok(true);
        }
        public async Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject loginRequest)
        {
            var account = await accountRepository.GetByUsernameAsync(loginRequest.UsernameOrEmail)
                       ?? await accountRepository.GetByEmailAsync(loginRequest.UsernameOrEmail);
            if (account == null)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail("Invalid username or password.");
            }
            if (account.IsSuspended)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail("This account has been suspended.");
            }
            if (!PasswordHasher.VerifyPassword(loginRequest.Password, account.PasswordHash))
            {
                await failedLoginRepository.IncrementAsync(account.Id);
                return ServiceResult<AccountProfileDataTransferObject>.Fail("Invalid username or password.");
            }
            await failedLoginRepository.ResetAsync(account.Id);
            string primaryRole = account.Roles?.FirstOrDefault()?.Name ?? StandardUserRoleName;
            sessionContext.Populate(account, primaryRole);
            return ServiceResult<AccountProfileDataTransferObject>.Ok(new AccountProfileDataTransferObject
            {
                Id = account.Id, Username = account.Username, DisplayName = account.DisplayName,
                Email = account.Email, Role = new RoleDataTransferObject { Name = primaryRole }
            });
        }
        public Task<ServiceResult<bool>> LogoutAsync()
        {
            sessionContext.Clear();
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        public Task<ServiceResult<string>> ForgotPasswordAsync() =>
            Task.FromResult(ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com."));
    }
}
