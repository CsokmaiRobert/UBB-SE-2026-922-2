using System;
using BoardRent.Data;
using BoardRent.Repositories;
using BoardRent.Services;
using BoardRent.ViewModels;
using BoardRent.Views;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BoardRent
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<AppDbContext>()
                .AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>()
                .AddSingleton<IUserRepository, UserRepository>()
                .AddSingleton<IFailedLoginRepository, FailedLoginRepository>()
                .AddSingleton<IAuthService, AuthService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IAdminService, AdminService>()
                .AddSingleton<IFilePickerService, FilePickerService>()
                .AddSingleton<ISessionContext, SessionContext>() // <--- ADĂUGAT CRITIC PENTRU DECUPLARE
                .AddTransient<LoginViewModel>()
                .AddTransient<RegisterViewModel>()
                .AddTransient<ProfileViewModel>()
                .AddTransient<AdminViewModel>()
                .BuildServiceProvider());
        }

        public static Window Window { get; private set; }

        private static Frame RootFrame { get; set; }

        public static void NavigateTo(Type pageType, bool clearBackStack = false)
        {
            RootFrame?.Navigate(pageType);
            if (clearBackStack && RootFrame != null)
            {
                RootFrame.BackStack.Clear();
            }
        }

        public static void NavigateBack()
        {
            if (RootFrame != null && RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Window = new MainWindow();
            RootFrame = new Frame();
            Window.Content = RootFrame;
            Window.Activate();

            var databaseContext = new AppDbContext();
            databaseContext.EnsureCreated();

            NavigateTo(typeof(LoginPage));
        }
    }
}