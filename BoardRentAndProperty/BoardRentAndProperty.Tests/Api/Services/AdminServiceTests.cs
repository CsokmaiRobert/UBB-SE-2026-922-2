using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class AdminServiceTests
    {
        private FakeAccountRepository accountRepository = null!;
        private FakeFailedLoginRepository failedLoginRepository = null!;
        private AdminService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountRepository = new FakeAccountRepository();
            this.failedLoginRepository = new FakeFailedLoginRepository();
            this.service = new AdminService(this.accountRepository, this.failedLoginRepository);
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

            this.accountRepository.Accounts = new List<Account> { account };
            this.failedLoginRepository.FailedLoginAttempts[accountId] = new FailedLoginAttempt
            {
                AccountId = accountId,
                LockedUntil = DateTime.UtcNow.AddMinutes(5),
            };

            var serviceResult = await this.service.GetAllAccountsAsync(1, 10);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Has.Count.EqualTo(1));
            Assert.That(serviceResult.Data![0].Username, Is.EqualTo("admin_user"));
            Assert.That(serviceResult.Data[0].Role.Name, Is.EqualTo("Administrator"));
            Assert.That(serviceResult.Data[0].IsLocked, Is.True);
        }

        [Test]
        public async Task SuspendAccountAsync_AccountExists_UpdatesStatusToSuspended()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, IsSuspended = false };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.SuspendAccountAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.IsSuspended, Is.True);
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task UnsuspendAccountAsync_AccountExists_UpdatesStatusToActive()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, IsSuspended = true };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.UnsuspendAccountAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.IsSuspended, Is.False);
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task ResetPasswordAsync_PasswordTooShort_ReturnsFailResult()
        {
            var serviceResult = await this.service.ResetPasswordAsync(Guid.NewGuid(), "123");

            Assert.That(serviceResult.Success, Is.False);
            Assert.That(serviceResult.Error, Does.Contain("at least 6 characters"));
        }

        [Test]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            string originalHash = "old_hash";
            var account = new Account { Id = accountId, PasswordHash = originalHash };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.ResetPasswordAsync(accountId, "NewSecurePass123!");

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.PasswordHash, Is.Not.EqualTo(originalHash));
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task UnlockAccountAsync_WhenCalled_ResetsFailedAttempts()
        {
            var accountId = Guid.NewGuid();

            var serviceResult = await this.service.UnlockAccountAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(this.failedLoginRepository.ResetCallCount, Is.EqualTo(1));
            Assert.That(this.failedLoginRepository.LastAccountId, Is.EqualTo(accountId));
        }
    }
}
