using System;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;

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
        public async Task GetProfileAsync_AccountDoesNotExist_ReturnsFailResult()
        {
            var accountId = Guid.NewGuid();

            this.accountRepository.AccountsById[accountId] = null;

            var serviceResult = await this.service.GetProfileAsync(accountId);

            Assert.That(serviceResult.Success, Is.False);
            Assert.That(serviceResult.Error, Is.EqualTo("Account not found."));
        }

        [Test]
        public async Task GetProfileAsync_AccountExists_ReturnsSuccessResultWithProfileData()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Username = "test_user",
                DisplayName = "Test User Display Name",
                Roles = { new Role { Id = Guid.NewGuid(), Name = "Standard User" } },
            };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.GetProfileAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Is.Not.Null);
            Assert.That(serviceResult.Data!.Username, Is.EqualTo("test_user"));
        }

        [Test]
        public async Task UpdateProfileAsync_ValidData_UpdatesAccountAndReturnsSuccess()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                DisplayName = "Original Name",
                Email = "original@test.com",
            };

            var updateData = new AccountProfileDataTransferObject
            {
                DisplayName = "Updated Display Name",
                Email = "updated@test.com",
            };

            this.accountRepository.AccountsById[accountId] = account;
            this.accountRepository.AccountsByEmail["updated@test.com"] = null;

            var serviceResult = await this.service.UpdateProfileAsync(accountId, updateData);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.DisplayName, Is.EqualTo("Updated Display Name"));
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task ChangePasswordAsync_ValidPasswords_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            string originalHash = PasswordHasher.HashPassword("OldPassword123!");
            var account = new Account
            {
                Id = accountId,
                PasswordHash = originalHash,
            };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.ChangePasswordAsync(accountId, "OldPassword123!", "NewSecurePass123!");

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.PasswordHash, Is.Not.EqualTo(originalHash));
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task SetAvatarUrlAsync_AccountExists_UpdatesAvatarUrl()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.SetAvatarUrlAsync(accountId, "/avatars/test.png");

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Is.EqualTo("/avatars/test.png"));
            Assert.That(account.AvatarUrl, Is.EqualTo("/avatars/test.png"));
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task RemoveAvatarAsync_AccountExists_ClearsAvatarUrlAndDeletesStoredFile()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                AvatarUrl = "/avatars/old.png",
            };

            this.accountRepository.AccountsById[accountId] = account;

            var serviceResult = await this.service.RemoveAvatarAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.AvatarUrl, Is.Empty);
            Assert.That(this.avatarStorageService.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.avatarStorageService.LastDeletedPath, Is.EqualTo("/avatars/old.png"));
            Assert.That(this.accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }
    }
}
