using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class AdminServiceTests
    {
        private Mock<IAccountRepository> accountRepositoryMock = null!;
        private Mock<IFailedLoginRepository> failedLoginRepositoryMock = null!;
        private AdminService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountRepositoryMock = new Mock<IAccountRepository>();
            this.failedLoginRepositoryMock = new Mock<IFailedLoginRepository>();
            this.service = new AdminService(this.accountRepositoryMock.Object, this.failedLoginRepositoryMock.Object);
        }

        [Test]
        public async Task GetAllAccountsAsync_WhenAccountsExist_ReturnsMappedProfiles()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Username = "admin_user",
                DisplayName = "Admin User",
                Email = "admin@test.com",
                IsSuspended = false,
                Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Administrator" } },
            };

            this.accountRepositoryMock
                .Setup(repository => repository.GetAllAsync(1, 10))
                .ReturnsAsync(new List<Account> { account });
            this.failedLoginRepositoryMock
                .Setup(repository => repository.GetByAccountIdAsync(accountId))
                .ReturnsAsync(new FailedLoginAttempt
                {
                    AccountId = accountId,
                    LockedUntil = DateTime.UtcNow.AddMinutes(5),
                });

            var serviceResult = await this.service.GetAllAccountsAsync(1, 10);

            serviceResult.Success.Should().BeTrue();
            serviceResult.Data.Should().HaveCount(1);
            serviceResult.Data![0].Username.Should().Be("admin_user");
            serviceResult.Data[0].Role.Name.Should().Be("Administrator");
            serviceResult.Data[0].IsLocked.Should().BeTrue();
        }

        [Test]
        public async Task SuspendAccountAsync_AccountExists_UpdatesStatusToSuspended()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, IsSuspended = false };

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.SuspendAccountAsync(accountId);

            serviceResult.Success.Should().BeTrue();
            account.IsSuspended.Should().BeTrue();
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
        }

        [Test]
        public async Task UnsuspendAccountAsync_AccountExists_UpdatesStatusToActive()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, IsSuspended = true };

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.UnsuspendAccountAsync(accountId);

            serviceResult.Success.Should().BeTrue();
            account.IsSuspended.Should().BeFalse();
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
        }

        [Test]
        public async Task ResetPasswordAsync_PasswordTooShort_ReturnsFailResult()
        {
            var serviceResult = await this.service.ResetPasswordAsync(Guid.NewGuid(), "123");

            serviceResult.Success.Should().BeFalse();
            serviceResult.Error.Should().Contain("at least 6 characters");
        }

        [Test]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            string originalHash = "old_hash";
            var account = new Account { Id = accountId, PasswordHash = originalHash };

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.ResetPasswordAsync(accountId, "NewSecurePass123!");

            serviceResult.Success.Should().BeTrue();
            account.PasswordHash.Should().NotBe(originalHash);
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
        }

        [Test]
        public async Task UnlockAccountAsync_WhenCalled_ResetsFailedAttempts()
        {
            var accountId = Guid.NewGuid();

            var serviceResult = await this.service.UnlockAccountAsync(accountId);

            serviceResult.Success.Should().BeTrue();
            this.failedLoginRepositoryMock.Verify(repository => repository.ResetAsync(accountId), Times.Once);
        }
    }
}
