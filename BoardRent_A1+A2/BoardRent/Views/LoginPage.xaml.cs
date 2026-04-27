namespace BoardRent.Views
{
    using System;
    using BoardRent.ViewModels;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<LoginViewModel>();
            this.DataContext = this.ViewModel;

            this.InitializeNavigationCallbacks();
        }

        public LoginViewModel ViewModel { get; }

        private void InitializeNavigationCallbacks()
        {
            this.ViewModel.OnLoginSuccess = (roleName) =>
            {
                if (roleName == "Administrator")
                {
                    App.NavigateTo(typeof(AdminPage), true);
                }
                else
                {
                    App.NavigateTo(typeof(ProfilePage), true);
                }
            };

            this.ViewModel.OnNavigateToRegister = () =>
            {
                App.NavigateTo(typeof(RegisterPage));
            };
        }

        private async void ForgotPassword_Click(object pointerSender, RoutedEventArgs eventArgs)
        {
            await this.ResetPasswordDialog.ShowAsync();
        }
    }
}