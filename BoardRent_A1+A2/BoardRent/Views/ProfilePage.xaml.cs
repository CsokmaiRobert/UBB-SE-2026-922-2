namespace BoardRent.Views
{
    using System;
    using BoardRent.ViewModels;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<ProfileViewModel>();
            this.DataContext = this.ViewModel;

            this.ViewModel.OnSignOutSuccess = () =>
            {
                App.NavigateTo(typeof(LoginPage), true);
            };

            this.Loaded += async (object sender, RoutedEventArgs eventArgs) =>
            {
                await this.ViewModel.LoadProfileAsync();
            };
        }

        public ProfileViewModel ViewModel { get; }
    }
}