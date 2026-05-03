using System;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Mappers;
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
    public sealed class AccountServiceTests
    {
        private Mock<IAccountRepository> accountRepositoryMock = null!;
        private Mock<IAvatarStorageService> avatarStorageServiceMock = null!;
        private AccountService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountRepositoryMock = new Mock<IAccountRepository>();
            this.avatarStorageServiceMock = new Mock<IAvatarStorageService>();
            this.service = new AccountService(
                this.accountRepositoryMock.Object,
                new AccountProfileMapper(),
                this.avatarStorageServiceMock.Object);
        }

        [Test]
        public async Task GetProfileAsync_AccountDoesNotExist_ReturnsFailResult()
        {
            var accountId = Guid.NewGuid();

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync((Account?)null);

            var serviceResult = await this.service.GetProfileAsync(accountId);

            serviceResult.Success.Should().BeFalse();
            serviceResult.Error.Should().Be("Account not found.");
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.GetProfileAsync(accountId);

            serviceResult.Success.Should().BeTrue();
            serviceResult.Data.Should().NotBeNull();
            serviceResult.Data!.Username.Should().Be("test_user");
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            this.accountRepositoryMock
                .Setup(repository => repository.GetByEmailAsync("updated@test.com"))
                .ReturnsAsync((Account?)null);

            var serviceResult = await this.service.UpdateProfileAsync(accountId, updateData);

            serviceResult.Success.Should().BeTrue();
            account.DisplayName.Should().Be("Updated Display Name");
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.ChangePasswordAsync(accountId, "OldPassword123!", "NewSecurePass123!");

            serviceResult.Success.Should().BeTrue();
            account.PasswordHash.Should().NotBe(originalHash);
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
        }

        [Test]
        public async Task SetAvatarUrlAsync_AccountExists_UpdatesAvatarUrl()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.SetAvatarUrlAsync(accountId, "/avatars/test.png");

            serviceResult.Success.Should().BeTrue();
            serviceResult.Data.Should().Be("/avatars/test.png");
            account.AvatarUrl.Should().Be("/avatars/test.png");
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
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

            this.accountRepositoryMock
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            var serviceResult = await this.service.RemoveAvatarAsync(accountId);

            serviceResult.Success.Should().BeTrue();
            account.AvatarUrl.Should().BeEmpty();
            this.avatarStorageServiceMock.Verify(service => service.Delete("/avatars/old.png"), Times.Once);
            this.accountRepositoryMock.Verify(repository => repository.UpdateAsync(account), Times.Once);
        }
    }
}
