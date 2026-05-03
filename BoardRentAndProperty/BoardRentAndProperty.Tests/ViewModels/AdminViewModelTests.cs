using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class AdminViewModelTests
    {
        private Mock<IAdminService> adminServiceMock = null!;
        private AdminViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.adminServiceMock = new Mock<IAdminService>();
            this.adminServiceMock
                .Setup(service => service.GetAllAccountsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ServiceResult<List<AccountProfileDataTransferObject>>.Ok(new List<AccountProfileDataTransferObject>()));

            this.systemUnderTest = new AdminViewModel(this.adminServiceMock.Object);
        }

        [Test]
        public async Task LoadAccountsAsync_ServiceReturnsData_PopulatesPagedItems()
        {
            var accounts = new List<AccountProfileDataTransferObject>
            {
                new AccountProfileDataTransferObject { Username = "user1", DisplayName = "User One" },
                new AccountProfileDataTransferObject { Username = "user2", DisplayName = "User Two" },
            };

            this.adminServiceMock
                .Setup(service => service.GetAllAccountsAsync(1, 3))
                .ReturnsAsync(ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accounts));

            await this.systemUnderTest.LoadAccountsAsync();

            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo("user1"));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void SelectedAccount_WhenChanged_EnablesCommands()
        {
            var selectedAccount = new AccountProfileDataTransferObject { Username = "target" };

            this.systemUnderTest.SelectedAccount = selectedAccount;

            Assert.That(this.systemUnderTest.SuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(this.systemUnderTest.UnsuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(this.systemUnderTest.UnlockAccountCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task SuspendAccountAsync_SelectedAccount_CallsServiceAndReloadsAccounts()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId, Username = "victim" };

            this.adminServiceMock
                .Setup(service => service.SuspendAccountAsync(accountId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.SuspendAccountCommand.ExecuteAsync(null);

            this.adminServiceMock.Verify(service => service.SuspendAccountAsync(accountId), Times.Once);
            this.adminServiceMock.Verify(service => service.GetAllAccountsAsync(1, 3), Times.Once);
        }

        [Test]
        public async Task NextPageCommand_WhenMultiplePagesExist_AdvancesCurrentPage()
        {
            var accounts = new List<AccountProfileDataTransferObject>
            {
                new AccountProfileDataTransferObject { Username = "user1" },
                new AccountProfileDataTransferObject { Username = "user2" },
                new AccountProfileDataTransferObject { Username = "user3" },
                new AccountProfileDataTransferObject { Username = "user4" },
            };

            this.adminServiceMock
                .Setup(service => service.GetAllAccountsAsync(1, 3))
                .ReturnsAsync(ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accounts));

            await this.systemUnderTest.LoadAccountsAsync();
            this.systemUnderTest.NextPageCommand.Execute(null);

            Assert.That(this.systemUnderTest.CurrentPage, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(1));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo("user4"));
        }

        [Test]
        public async Task UnlockAccountAsync_SuccessfulCall_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId };

            this.adminServiceMock
                .Setup(service => service.UnlockAccountAsync(accountId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.UnlockAccountCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Account unlocked."));
        }

        [Test]
        public async Task ResetPasswordWithValueAsync_ValidPassword_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId };
            string newPassword = "NewSecurePass123!";

            this.adminServiceMock
                .Setup(service => service.ResetPasswordAsync(accountId, newPassword))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.ResetPasswordWithValueAsync(newPassword);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Password reset successful."));
        }
    }
}
