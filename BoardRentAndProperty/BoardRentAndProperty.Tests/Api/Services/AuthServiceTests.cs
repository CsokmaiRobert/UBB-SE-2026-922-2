using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class AuthServiceTests
    {
        private FakeAccountRepository accountRepository = null!;
        private FakeFailedLoginRepository failedLoginRepository = null!;
        private AuthService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountRepository = new FakeAccountRepository();
            this.failedLoginRepository = new FakeFailedLoginRepository();
            this.service = new AuthService(this.accountRepository, this.failedLoginRepository);
        }

        [Test]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsFailResult()
        {
            var registrationRequest = new RegisterDataTransferObject
            {
                Username = "existing_user",
                Password = "Password123!",
            };

            this.accountRepository.AccountsByUsername["existing_user"] = new Account { Username = "existing_user" };

            var registrationResult = await this.service.RegisterAsync(registrationRequest);

            Assert.That(registrationResult.Success, Is.False);
            Assert.That(registrationResult.Error, Does.Contain("Username is already taken"));
        }

        [Test]
        public async Task RegisterAsync_ValidData_AddsAccountAndAssignsStandardRole()
        {
            Guid createdAccountId = Guid.Empty;
            var registrationRequest = new RegisterDataTransferObject
            {
                Username = "new_user",
                DisplayName = "New User",
                Email = "new@test.com",
                Password = "Password123!",
            };

            this.accountRepository.AccountsByUsername["new_user"] = null;

            var registrationResult = await this.service.RegisterAsync(registrationRequest);

            Assert.That(registrationResult.Success, Is.True);
            createdAccountId = this.accountRepository.LastAddedAccount!.Id;
            Assert.That(createdAccountId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(this.accountRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.AddRoleCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastRoleAccountId, Is.EqualTo(createdAccountId));
            Assert.That(this.accountRepository.LastRoleName, Is.EqualTo("Standard User"));
        }

        [Test]
        public async Task LoginAsync_SuspendedAccount_ReturnsFailResult()
        {
            var loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "suspended_user",
                Password = "AnyPassword123!",
            };

            var suspendedAccount = new Account
            {
                Username = "suspended_user",
                IsSuspended = true,
            };

            this.accountRepository.AccountsByUsername["suspended_user"] = suspendedAccount;

            var loginResult = await this.service.LoginAsync(loginRequest);

            Assert.That(loginResult.Success, Is.False);
            Assert.That(loginResult.Error, Is.EqualTo("This account has been suspended."));
        }

        [Test]
        public async Task LoginAsync_WrongPassword_IncrementsFailedAttempts()
        {
            string correctPassword = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";
            var accountId = Guid.NewGuid();

            var account = new Account
            {
                Id = accountId,
                Username = "test_user",
                PasswordHash = PasswordHasher.HashPassword(correctPassword),
                IsSuspended = false,
            };

            var loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "test_user",
                Password = wrongPassword,
            };

            this.accountRepository.AccountsByUsername["test_user"] = account;

            var loginResult = await this.service.LoginAsync(loginRequest);

            Assert.That(loginResult.Success, Is.False);
            Assert.That(this.failedLoginRepository.IncrementCallCount, Is.EqualTo(1));
            Assert.That(this.failedLoginRepository.LastAccountId, Is.EqualTo(accountId));
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ResetsFailedAttemptsAndReturnsProfile()
        {
            string password = "ValidPassword123!";
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Username = "valid_user",
                PasswordHash = PasswordHasher.HashPassword(password),
                IsSuspended = false,
                Roles = new List<Role> { new Role { Name = "Administrator" } },
            };

            var loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "valid_user",
                Password = password,
            };

            this.accountRepository.AccountsByUsername["valid_user"] = account;

            var loginResult = await this.service.LoginAsync(loginRequest);

            Assert.That(loginResult.Success, Is.True);
            Assert.That(loginResult.Data, Is.Not.Null);
            Assert.That(loginResult.Data!.Role.Name, Is.EqualTo("Administrator"));
            Assert.That(this.failedLoginRepository.ResetCallCount, Is.EqualTo(1));
            Assert.That(this.failedLoginRepository.LastAccountId, Is.EqualTo(accountId));
        }

        [Test]
        public async Task ForgotPasswordAsync_Always_ReturnsAdministratorContactMessage()
        {
            var serviceResult = await this.service.ForgotPasswordAsync();

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Does.Contain("admin@boardrent.com"));
        }

        [Test]
        public async Task LogoutAsync_WhenCalled_ReturnsSuccess()
        {
            var serviceResult = await this.service.LogoutAsync();

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Is.True);
        }
    }
}
