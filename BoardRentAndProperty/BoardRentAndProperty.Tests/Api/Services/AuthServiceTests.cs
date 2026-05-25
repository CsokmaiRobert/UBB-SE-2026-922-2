using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using AuthService = BoardRentAndProperty.Api.Services.AuthService;

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
            this.accountRepository.AccountsByUsername["existing_user"] = new Account { Username = "existing_user" };

            var registrationResult = await this.service.RegisterAsync(new RegisterDataTransferObject
            {
                Username = "existing_user",
                Password = "Password123!",
            });

            Assert.That(registrationResult.Success, Is.False);
            Assert.That(registrationResult.Error, Does.Contain("Username is already taken"));
        }

        [Test]
        public async Task RegisterAsync_ValidData_AddsAccountAndAssignsStandardRole()
        {
            this.accountRepository.AccountsByUsername["new_user"] = null;

            var registrationResult = await this.service.RegisterAsync(new RegisterDataTransferObject
            {
                Username = "new_user",
                DisplayName = "New User",
                Email = "new@test.com",
                Password = "Password123!",
            });

            Assert.That(registrationResult.Success, Is.True);
            Assert.That(this.accountRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastRoleName, Is.EqualTo("Standard User"));
        }

        [Test]
        public async Task LoginAsync_SuspendedAccount_ReturnsFailResult()
        {
            this.accountRepository.AccountsByUsername["suspended_user"] = new Account
            {
                Username = "suspended_user",
                IsSuspended = true,
            };

            var loginResult = await this.service.LoginAsync(new LoginDataTransferObject
            {
                UsernameOrEmail = "suspended_user",
                Password = "AnyPassword123!",
            });

            Assert.That(loginResult.Success, Is.False);
            Assert.That(loginResult.Error, Is.EqualTo("This account has been suspended."));
        }

        [Test]
        public async Task LoginAsync_WrongPassword_IncrementsFailedAttempts()
        {
            string correctPassword = "CorrectPassword123!";
            var accountId = Guid.NewGuid();

            this.accountRepository.AccountsByUsername["test_user"] = new Account
            {
                Id = accountId,
                Username = "test_user",
                PasswordHash = PasswordHasher.HashPassword(correctPassword),
            };

            var loginResult = await this.service.LoginAsync(new LoginDataTransferObject
            {
                UsernameOrEmail = "test_user",
                Password = "WrongPassword123!",
            });

            Assert.That(loginResult.Success, Is.False);
            Assert.That(this.failedLoginRepository.IncrementCallCount, Is.EqualTo(1));
            Assert.That(this.failedLoginRepository.LastAccountId, Is.EqualTo(accountId));
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ResetsFailedAttemptsAndReturnsProfile()
        {
            string password = "ValidPassword123!";
            var accountId = Guid.NewGuid();
            this.accountRepository.AccountsByUsername["valid_user"] = new Account
            {
                Id = accountId,
                Username = "valid_user",
                PasswordHash = PasswordHasher.HashPassword(password),
                Roles = new List<Role> { new Role { Name = "Administrator" } },
            };

            var loginResult = await this.service.LoginAsync(new LoginDataTransferObject
            {
                UsernameOrEmail = "valid_user",
                Password = password,
            });

            Assert.That(loginResult.Success, Is.True);
            Assert.That(loginResult.Data!.Role.Name, Is.EqualTo("Administrator"));
            Assert.That(this.failedLoginRepository.ResetCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ForgotPasswordAndLogout_AlwaysSucceedWithExpectedPayloads()
        {
            var forgotResult = await this.service.ForgotPasswordAsync();
            Assert.That(forgotResult.Success, Is.True);
            Assert.That(forgotResult.Data, Does.Contain("admin@boardrent.com"));

            var logoutResult = await this.service.LogoutAsync();
            Assert.That(logoutResult.Success, Is.True);
            Assert.That(logoutResult.Data, Is.True);
        }
    }
}
