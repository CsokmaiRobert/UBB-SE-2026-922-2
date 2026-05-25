using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using AdminService = BoardRentAndProperty.Api.Services.AdminService;

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
        public async Task GetAllAccountsAsync_WhenAccountsExist_ReturnsMappedProfilesWithLockedFlag()
        {
            var accountId = Guid.NewGuid();
            this.accountRepository.Accounts = new List<Account>
            {
                new Account
                {
                    Id = accountId,
                    Username = "admin_user",
                    Email = "admin@test.com",
                    Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Administrator" } },
                },
            };
            this.failedLoginRepository.FailedLoginAttempts[accountId] = new FailedLoginAttempt
            {
                AccountId = accountId,
                LockedUntil = DateTime.UtcNow.AddMinutes(5),
            };

            var serviceResult = await this.service.GetAllAccountsAsync(1, 10);

            Assert.That(serviceResult.Data![0].Username, Is.EqualTo("admin_user"));
            Assert.That(serviceResult.Data[0].Role.Name, Is.EqualTo("Administrator"));
            Assert.That(serviceResult.Data[0].IsLocked, Is.True);
        }

        [Test]
        public async Task SuspendAndUnsuspendAccountAsync_FlipIsSuspendedFlagAndCallUpdate()
        {
            var accountToSuspend = new Account { Id = Guid.NewGuid(), IsSuspended = false };
            this.accountRepository.AccountsById[accountToSuspend.Id] = accountToSuspend;
            await this.service.SuspendAccountAsync(accountToSuspend.Id);
            Assert.That(accountToSuspend.IsSuspended, Is.True);

            var accountToUnsuspend = new Account { Id = Guid.NewGuid(), IsSuspended = true };
            this.accountRepository.AccountsById[accountToUnsuspend.Id] = accountToUnsuspend;
            await this.service.UnsuspendAccountAsync(accountToUnsuspend.Id);
            Assert.That(accountToUnsuspend.IsSuspended, Is.False);

            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(2));
        }

        [Test]
        public async Task ResetPasswordAsync_HandlesValidationAndUpdatesPasswordHash()
        {
            var tooShortResult = await this.service.ResetPasswordAsync(Guid.NewGuid(), "123");
            Assert.That(tooShortResult.Success, Is.False);
            Assert.That(tooShortResult.Error, Does.Contain("at least 6 characters"));

            var accountId = Guid.NewGuid();
            string originalHash = "old_hash";
            this.accountRepository.AccountsById[accountId] = new Account { Id = accountId, PasswordHash = originalHash };

            var validResult = await this.service.ResetPasswordAsync(accountId, "NewSecurePass123!");
            Assert.That(validResult.Success, Is.True);
            Assert.That(this.accountRepository.AccountsById[accountId]!.PasswordHash, Is.Not.EqualTo(originalHash));
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
