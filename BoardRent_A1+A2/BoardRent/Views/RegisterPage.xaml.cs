namespace BoardRent.Views
{
    using System;
    using BoardRent.ViewModels;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<RegisterViewModel>();
            this.DataContext = this.ViewModel;

            this.InitializeNavigationCallbacks();
        }

        public RegisterViewModel ViewModel { get; }

        private void InitializeNavigationCallbacks()
        {
            this.ViewModel.OnRegistrationSuccess = () =>
            {
                App.NavigateTo(typeof(ProfilePage), true);
            };

            this.ViewModel.OnNavigateBackRequest = () =>
            {
                App.NavigateBack();
            };
        }
    }
}