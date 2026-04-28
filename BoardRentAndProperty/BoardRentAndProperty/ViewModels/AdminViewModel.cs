namespace BoardRentAndProperty.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Services;
    using BoardRentAndProperty.Utilities;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class AdminViewModel : BaseViewModel
    {
        private const int DefaultPageSize = 10;
        private readonly IAdminService adminService;

        [ObservableProperty]
        private ObservableCollection<AccountProfileDataTransferObject> accounts = new ObservableCollection<AccountProfileDataTransferObject>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SuspendAccountCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnsuspendAccountCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetPasswordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnlockAccountCommand))]
        private AccountProfileDataTransferObject selectedAccount;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int totalPages = 1;

        public AdminViewModel(IAdminService adminService)
        {
            this.adminService = adminService;

            TaskUtilities.FireAndForgetSafeAsync(this.LoadAccountsAsync());
        }

        public async Task LoadAccountsAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            var serviceResult = await this.adminService.GetAllAccountsAsync(this.CurrentPage, DefaultPageSize);

            if (serviceResult.Success && serviceResult.Data != null)
            {
                this.Accounts = new ObservableCollection<AccountProfileDataTransferObject>(serviceResult.Data);

                this.TotalPages = serviceResult.Data.Count == DefaultPageSize
                    ? this.CurrentPage + 1
                    : this.CurrentPage;
            }
            else
            {
                this.ErrorMessage = serviceResult.Error ?? "Failed to load accounts.";
            }

            this.IsLoading = false;
        }

        public async Task ResetPasswordWithValueAsync(string newPassword)
        {
            if (this.SelectedAccount == null)
            {
                return;
            }

            var serviceResult = await this.adminService.ResetPasswordAsync(this.SelectedAccount.Id, newPassword);

            if (serviceResult.Success)
            {
                this.ErrorMessage = "Password has been reset successfully.";
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedAccount))]
        private async Task SuspendAccountAsync()
        {
            if (this.SelectedAccount == null)
            {
                return;
            }

            var serviceResult = await this.adminService.SuspendAccountAsync(this.SelectedAccount.Id);

            if (serviceResult.Success)
            {
                this.SelectedAccount.IsSuspended = true;
                await this.LoadAccountsAsync();
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedAccount))]
        private async Task UnsuspendAccountAsync()
        {
            if (this.SelectedAccount == null)
            {
                return;
            }

            var serviceResult = await this.adminService.UnsuspendAccountAsync(this.SelectedAccount.Id);

            if (serviceResult.Success)
            {
                this.SelectedAccount.IsSuspended = false;
                await this.LoadAccountsAsync();
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedAccount))]
        private async Task ResetPasswordAsync()
        {
            await Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedAccount))]
        private async Task UnlockAccountAsync()
        {
            if (this.SelectedAccount == null)
            {
                return;
            }

            var serviceResult = await this.adminService.UnlockAccountAsync(this.SelectedAccount.Id);

            if (serviceResult.Success)
            {
                this.ErrorMessage = "Account unlocked successfully.";
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private async Task PreviousPageAsync()
        {
            if (this.CurrentPage > 1)
            {
                this.CurrentPage--;
                await this.LoadAccountsAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task NextPageAsync()
        {
            if (this.CurrentPage < this.TotalPages)
            {
                this.CurrentPage++;
                await this.LoadAccountsAsync();
            }
        }

        private bool CanModifySelectedAccount()
        {
            return this.SelectedAccount != null;
        }

        private bool CanGoToPreviousPage()
        {
            return this.CurrentPage > 1;
        }

        private bool CanGoToNextPage()
        {
            return this.CurrentPage < this.TotalPages;
        }
    }
}
