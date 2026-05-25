using System;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using AccountService = BoardRentAndProperty.Api.Services.AccountService;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class AccountServiceTests
    {
        private FakeAccountRepository accountRepository = null!;
        private FakeAvatarStorageService avatarStorageService = null!;
        private AccountService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountRepository = new FakeAccountRepository();
            this.avatarStorageService = new FakeAvatarStorageService();
            this.service = new AccountService(
                this.accountRepository,
                new AccountProfileMapper(),
                this.avatarStorageService);
        }

        [Test]
        public async Task GetProfileAsync_AccountMissingOrExisting_ReturnsAppropriateResult()
        {
            var missingAccountId = Guid.NewGuid();
            this.accountRepository.AccountsById[missingAccountId] = null;
            var missingResult = await this.service.GetProfileAsync(missingAccountId);
            Assert.That(missingResult.Success, Is.False);
            Assert.That(missingResult.Error, Is.EqualTo("Account not found."));

            var existingAccountId = Guid.NewGuid();
            this.accountRepository.AccountsById[existingAccountId] = new Account
            {
                Id = existingAccountId,
                Username = "test_user",
                DisplayName = "Test User",
                Roles = { new Role { Id = Guid.NewGuid(), Name = "Standard User" } },
            };
            var existingResult = await this.service.GetProfileAsync(existingAccountId);
            Assert.That(existingResult.Success, Is.True);
            Assert.That(existingResult.Data!.Username, Is.EqualTo("test_user"));
        }

        [Test]
        public async Task UpdateProfileAsync_ValidData_UpdatesAccountAndReturnsSuccess()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, DisplayName = "Original Name", Email = "original@test.com" };
            this.accountRepository.AccountsById[accountId] = account;
            this.accountRepository.AccountsByEmail["updated@test.com"] = null;

            var updateResult = await this.service.UpdateProfileAsync(accountId, new AccountProfileDataTransferObject
            {
                DisplayName = "Updated Display Name",
                Email = "updated@test.com",
            });

            Assert.That(updateResult.Success, Is.True);
            Assert.That(account.DisplayName, Is.EqualTo("Updated Display Name"));
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ChangePasswordAsync_ValidPasswords_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            string originalHash = PasswordHasher.HashPassword("OldPassword123!");
            var account = new Account { Id = accountId, PasswordHash = originalHash };
            this.accountRepository.AccountsById[accountId] = account;

            var changeResult = await this.service.ChangePasswordAsync(accountId, "OldPassword123!", "NewSecurePass123!");

            Assert.That(changeResult.Success, Is.True);
            Assert.That(account.PasswordHash, Is.Not.EqualTo(originalHash));
        }

        [Test]
        public async Task SetAvatarUrlAsync_AccountExists_UpdatesAvatarUrl()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            this.accountRepository.AccountsById[accountId] = account;

            var setResult = await this.service.SetAvatarUrlAsync(accountId, "/avatars/test.png");

            Assert.That(setResult.Success, Is.True);
            Assert.That(account.AvatarUrl, Is.EqualTo("/avatars/test.png"));
        }

        [Test]
        public async Task RemoveAvatarAsync_AccountExists_ClearsAvatarUrlAndDeletesStoredFile()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, AvatarUrl = "/avatars/old.png" };
            this.accountRepository.AccountsById[accountId] = account;

            var removeResult = await this.service.RemoveAvatarAsync(accountId);

            Assert.That(removeResult.Success, Is.True);
            Assert.That(account.AvatarUrl, Is.Empty);
            Assert.That(this.avatarStorageService.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.avatarStorageService.LastDeletedPath, Is.EqualTo("/avatars/old.png"));
        }
    }
}
