namespace BoardRentAndProperty.Views
{
    using System;
    using System.ComponentModel;
    using BoardRentAndProperty.Services;
    using BoardRentAndProperty.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class AdminPage : Page, INotifyPropertyChanged
    {
        private readonly IDesktopAuthorizationService authorizationService;

        public AdminPage()
        {
            this.InitializeComponent();
            this.authorizationService = App.Services.GetRequiredService<IDesktopAuthorizationService>();
            this.ViewModel = App.Services.GetRequiredService<AdminViewModel>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AdminViewModel ViewModel { get; }

        public bool IsUnauthorized => !this.authorizationService.IsAdministrator;

        public Visibility IsAuthorizedVisibility => this.IsUnauthorized ? Visibility.Collapsed : Visibility.Visible;

        public bool IsErrorVisible => this.ViewModel != null && !string.IsNullOrEmpty(this.ViewModel.ErrorMessage);

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (!this.authorizationService.IsLoggedIn)
            {
                App.OnUserLoggedOut();
                return;
            }

            if (!this.IsUnauthorized)
            {
                this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
                await this.ViewModel.LoadAccountsAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedFrom(navigationEventArgs);
            this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(AdminViewModel.ErrorMessage))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsErrorVisible)));
            }
        }

        private void OnSignOutClicked(object sender, RoutedEventArgs eventArgs)
        {
            App.OnUserLoggedOut();
        }

        private async void OnResetPasswordClicked(object sender, RoutedEventArgs eventArgs)
        {
            if (this.ViewModel.SelectedAccount == null)
            {
                return;
            }

            ContentDialog resetPasswordDialog = new ContentDialog
            {
                Title = $"Reset password for {this.ViewModel.SelectedAccount.Username}",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            PasswordBox newPasswordBox = new PasswordBox
            {
                PlaceholderText = "Enter new password"
            };

            resetPasswordDialog.Content = newPasswordBox;

            ContentDialogResult dialogResult = await resetPasswordDialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(newPasswordBox.Password))
            {
                await this.ViewModel.ResetPasswordWithValueAsync(newPasswordBox.Password);
            }
        }
    }
}
