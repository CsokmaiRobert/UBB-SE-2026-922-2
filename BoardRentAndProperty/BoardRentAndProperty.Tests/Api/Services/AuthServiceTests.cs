using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class AuthServiceTests
    {
        private Mock<IAccountRepository> accountRepositoryMock = null!;
        private Mock<IFailedLoginRepository> failedLoginRepositoryMock = null!;
        private AuthService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountRepositoryMock = new Mock<IAccountRepository>();
            this.failedLoginRepositoryMock = new Mock<IFailedLoginRepository>();
            this.service = new AuthService(this.accountRepositoryMock.Object, this.failedLoginRepositoryMock.Object);
        }

        [Test]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsFailResult()
        {
            var registrationRequest = new RegisterDataTransferObject
            {
                Username = "existing_user",
                Password = "Password123!",
            };

            this.accountRepositoryMock
                .Setup(repository => repository.GetByUsernameAsync("existing_user"))
                .ReturnsAsync(new Account { Username = "existing_user" });

            var registrationResult = await this.service.RegisterAsync(registrationRequest);

            registrationResult.Success.Should().BeFalse();
            registrationResult.Error.Should().Contain("Username is already taken");
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByUsernameAsync("new_user"))
                .ReturnsAsync((Account?)null);
            this.accountRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<Account>()))
                .Callback<Account>(account => createdAccountId = account.Id)
                .Returns(Task.CompletedTask);

            var registrationResult = await this.service.RegisterAsync(registrationRequest);

            registrationResult.Success.Should().BeTrue();
            createdAccountId.Should().NotBe(Guid.Empty);
            this.accountRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Account>()), Times.Once);
            this.accountRepositoryMock.Verify(repository => repository.AddRoleAsync(createdAccountId, "Standard User"), Times.Once);
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByUsernameAsync("suspended_user"))
                .ReturnsAsync(suspendedAccount);

            var loginResult = await this.service.LoginAsync(loginRequest);

            loginResult.Success.Should().BeFalse();
            loginResult.Error.Should().Be("This account has been suspended.");
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByUsernameAsync("test_user"))
                .ReturnsAsync(account);

            var loginResult = await this.service.LoginAsync(loginRequest);

            loginResult.Success.Should().BeFalse();
            this.failedLoginRepositoryMock.Verify(repository => repository.IncrementAsync(accountId), Times.Once);
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByUsernameAsync("valid_user"))
                .ReturnsAsync(account);

            var loginResult = await this.service.LoginAsync(loginRequest);

            loginResult.Success.Should().BeTrue();
            loginResult.Data.Should().NotBeNull();
            loginResult.Data!.Role.Name.Should().Be("Administrator");
            this.failedLoginRepositoryMock.Verify(repository => repository.ResetAsync(accountId), Times.Once);
        }

        [Test]
        public async Task ForgotPasswordAsync_Always_ReturnsAdministratorContactMessage()
        {
            var serviceResult = await this.service.ForgotPasswordAsync();

            serviceResult.Success.Should().BeTrue();
            serviceResult.Data.Should().Contain("admin@boardrent.com");
        }

        [Test]
        public async Task LogoutAsync_WhenCalled_ReturnsSuccess()
        {
            var serviceResult = await this.service.LogoutAsync();

            serviceResult.Success.Should().BeTrue();
            serviceResult.Data.Should().BeTrue();
        }
    }
}
