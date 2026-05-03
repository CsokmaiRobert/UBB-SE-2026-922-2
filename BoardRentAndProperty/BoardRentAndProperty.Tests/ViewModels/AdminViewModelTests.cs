using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class AdminViewModelTests
    {
        private FakeClientAdminService adminService = null!;
        private AdminViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.adminService = new FakeClientAdminService();
            this.systemUnderTest = new AdminViewModel(this.adminService);
        }

        [Test]
        public async Task LoadAccountsAsync_ServiceReturnsData_PopulatesPagedItems()
        {
            int pageSize = PagedViewModel<AccountProfileDataTransferObject>.PageSize;
            var accounts = new List<AccountProfileDataTransferObject>
            {
                new AccountProfileDataTransferObject { Username = "user1", DisplayName = "User One" },
                new AccountProfileDataTransferObject { Username = "user2", DisplayName = "User Two" },
            };

            this.adminService.AccountsResult =
                ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accounts);

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
            int pageSize = PagedViewModel<AccountProfileDataTransferObject>.PageSize;
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId, Username = "victim" };

            this.adminService.SuspendResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.SuspendAccountCommand.ExecuteAsync(null);

            Assert.That(this.adminService.SuspendCallCount, Is.EqualTo(1));
            Assert.That(this.adminService.GetAllAccountsCallCount, Is.EqualTo(1));
            Assert.That(this.adminService.LastPage, Is.EqualTo(1));
            Assert.That(this.adminService.LastPageSize, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task NextPageCommand_WhenMultiplePagesExist_AdvancesCurrentPage()
        {
            int pageSize = PagedViewModel<AccountProfileDataTransferObject>.PageSize;
            var accounts = new List<AccountProfileDataTransferObject>();
            for (int accountIndex = 1; accountIndex <= pageSize + 1; accountIndex++)
            {
                accounts.Add(new AccountProfileDataTransferObject { Username = $"user{accountIndex}" });
            }

            this.adminService.AccountsResult =
                ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accounts);

            await this.systemUnderTest.LoadAccountsAsync();
            this.systemUnderTest.NextPageCommand.Execute(null);

            Assert.That(this.systemUnderTest.CurrentPage, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(1));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo($"user{pageSize + 1}"));
        }

        [Test]
        public async Task UnlockAccountAsync_SuccessfulCall_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId };

            this.adminService.UnlockResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.UnlockAccountCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Account unlocked."));
        }

        [Test]
        public async Task ResetPasswordWithValueAsync_ValidPassword_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId };
            string newPassword = "NewSecurePass123!";

            this.adminService.ResetPasswordResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.ResetPasswordWithValueAsync(newPassword);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Password reset successful."));
        }
    }
}
