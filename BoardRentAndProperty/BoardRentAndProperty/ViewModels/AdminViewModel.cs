namespace BoardRentAndProperty.ViewModels
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Services;
    using CommunityToolkit.Mvvm.Input;

    public class AdminViewModel : PagedViewModel<AccountProfileDataTransferObject>
    {
        private readonly IAdminService adminService;
        private AccountProfileDataTransferObject selectedAccount;
        private string errorMessage;
        private bool isLoading;

        public AdminViewModel(IAdminService adminService)
        {
            this.adminService = adminService;

            this.SuspendAccountCommand = new AsyncRelayCommand(this.SuspendAccountAsync, this.CanModifySelectedAccount);
            this.UnsuspendAccountCommand = new AsyncRelayCommand(this.UnsuspendAccountAsync, this.CanModifySelectedAccount);
            this.UnlockAccountCommand = new AsyncRelayCommand(this.UnlockAccountAsync, this.CanModifySelectedAccount);
            this.NextPageCommand = new RelayCommand(this.ExecuteNextPage);
            this.PreviousPageCommand = new RelayCommand(this.ExecutePreviousPage);
        }

        public IAsyncRelayCommand SuspendAccountCommand { get; }
        public IAsyncRelayCommand UnsuspendAccountCommand { get; }
        public IAsyncRelayCommand UnlockAccountCommand { get; }
        public IRelayCommand NextPageCommand { get; }
        public IRelayCommand PreviousPageCommand { get; }

        public AccountProfileDataTransferObject SelectedAccount
        {
            get => this.selectedAccount;
            set
            {
                if (this.selectedAccount != value)
                {
                    this.selectedAccount = value;
                    this.OnPropertyChanged(nameof(this.SelectedAccount));
                    this.SuspendAccountCommand.NotifyCanExecuteChanged();
                    this.UnsuspendAccountCommand.NotifyCanExecuteChanged();
                    this.UnlockAccountCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                if (this.errorMessage != value)
                {
                    this.errorMessage = value;
                    this.OnPropertyChanged(nameof(this.ErrorMessage));
                }
            }
        }

        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    this.OnPropertyChanged(nameof(this.IsLoading));
                }
            }
        }

        protected override void Reload()
        {
            _ = this.LoadAccountsAsync();
        }

        public async Task LoadAccountsAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            var serviceResult = await this.adminService.GetAllAccountsAsync(this.CurrentPage, PageSize);

            if (serviceResult.Success && serviceResult.Data != null)
            {
                this.SetAllItems(serviceResult.Data.ToImmutableList());
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
            this.ErrorMessage = serviceResult.Success ? "Password reset successful." : serviceResult.Error;
        }

        private async Task SuspendAccountAsync()
        {
            var result = await this.adminService.SuspendAccountAsync(this.SelectedAccount.Id);
            if (result.Success)
            {
                await this.LoadAccountsAsync();
            }
            else
            {
                this.ErrorMessage = result.Error;
            }
        }

        private async Task UnsuspendAccountAsync()
        {
            var result = await this.adminService.UnsuspendAccountAsync(this.SelectedAccount.Id);
            if (result.Success)
            {
                await this.LoadAccountsAsync();
            }
            else
            {
                this.ErrorMessage = result.Error;
            }
        }

        private async Task UnlockAccountAsync()
        {
            var result = await this.adminService.UnlockAccountAsync(this.SelectedAccount.Id);
            this.ErrorMessage = result.Success ? "Account unlocked." : result.Error;
        }

        private void ExecuteNextPage() => this.NextPage();

        private void ExecutePreviousPage() => this.PrevPage();

        private bool CanModifySelectedAccount() => this.SelectedAccount != null;
    }
}