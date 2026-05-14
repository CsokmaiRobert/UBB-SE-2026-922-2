using System;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Services
{
    public class AuthService : IAuthService
    {
        private const string StandardUserRoleName = "Standard User";

        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;

        public AuthService(IAccountRepository accountRepository, IFailedLoginRepository failedLoginRepository)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            var existingByUsername = await this.accountRepository.GetByUsernameAsync(registrationRequest.Username);
            if (existingByUsername != null)
            {
                return ServiceResult<bool>.Fail("Username|Username is already taken.");
            }

            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                DisplayName = registrationRequest.DisplayName,
                Username = registrationRequest.Username,
                Email = registrationRequest.Email,
                PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
                PhoneNumber = registrationRequest.PhoneNumber ?? string.Empty,
                AvatarUrl = string.Empty,
                Country = registrationRequest.Country ?? string.Empty,
                City = registrationRequest.City ?? string.Empty,
                StreetName = registrationRequest.StreetName ?? string.Empty,
                StreetNumber = registrationRequest.StreetNumber ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSuspended = false,
            };

            await this.accountRepository.AddAsync(newAccount);
            await this.accountRepository.AddRoleAsync(newAccount.Id, StandardUserRoleName);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject loginRequest)
        {
            var account = await this.accountRepository.GetByUsernameAsync(loginRequest.UsernameOrEmail)
                       ?? await this.accountRepository.GetByEmailAsync(loginRequest.UsernameOrEmail);
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
                await this.failedLoginRepository.IncrementAsync(account.Id);
                return ServiceResult<AccountProfileDataTransferObject>.Fail("Invalid username or password.");
            }

            await this.failedLoginRepository.ResetAsync(account.Id);
            string primaryRole = account.Roles?.FirstOrDefault()?.Name ?? StandardUserRoleName;

            return ServiceResult<AccountProfileDataTransferObject>.Ok(new AccountProfileDataTransferObject
            {
                Id = account.Id,
                Username = account.Username,
                DisplayName = account.DisplayName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                AvatarUrl = account.AvatarUrl,
                Country = account.Country,
                City = account.City,
                StreetName = account.StreetName,
                StreetNumber = account.StreetNumber,
                IsSuspended = account.IsSuspended,
                Role = new RoleDataTransferObject { Name = primaryRole },
            });
        }

        public Task<ServiceResult<bool>> LogoutAsync() =>
            Task.FromResult(ServiceResult<bool>.Ok(true));

        public Task<ServiceResult<string>> ForgotPasswordAsync() =>
            Task.FromResult(ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com."));
    }
}
