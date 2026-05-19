namespace BoardRentAndProperty.ViewModels
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using BoardRentAndProperty.ApiClient;
    using BoardRentAndProperty.Contracts.DataTransferObjects;
    using BoardRentAndProperty.Services;
    using CommunityToolkit.Mvvm.Input;

    public class AdminViewModel : PagedViewModel<AccountProfileDataTransferObject>
    {
        private const string AdminAccessDeniedMessage = "Unauthorized access. Administrator role is required.";

        private readonly IAdminService adminService;
        private readonly IDesktopAuthorizationService authorizationService;
        private AccountProfileDataTransferObject selectedAccount;
        private string errorMessage;
        private bool isLoading;

        public AdminViewModel(IAdminService adminService, IDesktopAuthorizationService authorizationService)
        {
            this.adminService = adminService;
            this.authorizationService = authorizationService;

            this.SuspendAccountCommand = new AsyncRelayCommand(this.SuspendAccountAsync, this.CanModifySelectedAccount);
            this.UnsuspendAccountCommand = new AsyncRelayCommand(this.UnsuspendAccountAsync, this.CanModifySelectedAccount);
            this.UnlockAccountCommand = new AsyncRelayCommand(this.UnlockAccountAsync, this.CanModifySelectedAccount);
            this.NextPageCommand = new RelayCommand(this.ExecuteNextPage);
            this.PreviousPageCommand = new RelayCommand(this.ExecutePreviousPage);
        }

        public AdminViewModel(IAdminService adminService)
            : this(adminService, new AlwaysAuthorizedDesktopAuthorizationService())
        {
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

            if (!this.authorizationService.IsAdministrator)
            {
                this.ErrorMessage = AdminAccessDeniedMessage;
                this.IsLoading = false;
                return;
            }

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

            if (!this.authorizationService.IsAdministrator)
            {
                this.ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

            var serviceResult = await this.adminService.ResetPasswordAsync(this.SelectedAccount.Id, newPassword);
            this.ErrorMessage = serviceResult.Success ? "Password reset successful." : serviceResult.Error;
        }

        private async Task SuspendAccountAsync()
        {
            if (!this.authorizationService.IsAdministrator)
            {
                this.ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

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
            if (!this.authorizationService.IsAdministrator)
            {
                this.ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

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
            if (!this.authorizationService.IsAdministrator)
            {
                this.ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

            var result = await this.adminService.UnlockAccountAsync(this.SelectedAccount.Id);
            this.ErrorMessage = result.Success ? "Account unlocked." : result.Error;
        }

        private void ExecuteNextPage() => this.NextPage();

        private void ExecutePreviousPage() => this.PrevPage();

        private bool CanModifySelectedAccount() => this.SelectedAccount != null;

        private sealed class AlwaysAuthorizedDesktopAuthorizationService : IDesktopAuthorizationService
        {
            public Guid CurrentAccountId => Guid.Empty;

            public bool IsLoggedIn => true;

            public bool IsAdministrator => true;

            public bool CanAccessPage(Type pageType) => true;

            public bool CanAccessMenuPage(AppPage page) => true;
        }
    }
}
