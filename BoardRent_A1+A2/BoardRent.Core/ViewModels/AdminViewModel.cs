namespace BoardRent.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class AdminViewModel : BaseViewModel
    {
        private const int DefaultPageSize = 10;
        private readonly IAdminService adminService;

        [ObservableProperty]
        private ObservableCollection<UserProfileDataTransferObject> users = new ObservableCollection<UserProfileDataTransferObject>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnsuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetPasswordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnlockAccountCommand))]
        private UserProfileDataTransferObject selectedUser;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int totalPages = 1;

        public AdminViewModel(IAdminService adminService)
        {
            this.adminService = adminService;

            // Folosim numele complet pentru extensie
            TaskUtilities.FireAndForgetSafeAsync(this.LoadUsersAsync());
        }

        public async Task LoadUsersAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            var serviceResult = await this.adminService.GetAllUsersAsync(this.CurrentPage, DefaultPageSize);

            if (serviceResult.Success && serviceResult.Data != null)
            {
                this.Users = new ObservableCollection<UserProfileDataTransferObject>(serviceResult.Data);

                // Logică pentru calcularea paginilor fără numere magice
                this.TotalPages = serviceResult.Data.Count == DefaultPageSize
                    ? this.CurrentPage + 1
                    : this.CurrentPage;
            }
            else
            {
                this.ErrorMessage = serviceResult.Error ?? "Failed to load users.";
            }

            this.IsLoading = false;
        }

        public async Task ResetPasswordWithValueAsync(string newPassword)
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult = await this.adminService.ResetPasswordAsync(this.SelectedUser.Id, newPassword);

            if (serviceResult.Success)
            {
                this.ErrorMessage = "Password has been reset successfully.";
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task SuspendUserAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult = await this.adminService.SuspendUserAsync(this.SelectedUser.Id);

            if (serviceResult.Success)
            {
                this.SelectedUser.IsSuspended = true;
                await this.LoadUsersAsync();
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnsuspendUserAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult = await this.adminService.UnsuspendUserAsync(this.SelectedUser.Id);

            if (serviceResult.Success)
            {
                this.SelectedUser.IsSuspended = false;
                await this.LoadUsersAsync();
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task ResetPasswordAsync()
        {
            // Metodă asincronă goală pentru activarea comenzii în interfață
            await Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnlockAccountAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult = await this.adminService.UnlockAccountAsync(this.SelectedUser.Id);

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
                await this.LoadUsersAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task NextPageAsync()
        {
            if (this.CurrentPage < this.TotalPages)
            {
                this.CurrentPage++;
                await this.LoadUsersAsync();
            }
        }

        private bool CanModifySelectedUser()
        {
            return this.SelectedUser != null;
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