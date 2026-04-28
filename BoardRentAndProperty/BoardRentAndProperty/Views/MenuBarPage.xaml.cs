namespace BoardRentAndProperty.Views
{
    using System;
    using System.Collections.Generic;
    using BoardRentAndProperty.Services;
    using BoardRentAndProperty.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class MenuBarPage : Page
    {
        private static readonly Dictionary<AppPage, Type> PageTypeMap = new ()
        {
            { AppPage.Listings,            typeof(ListingsPage) },
            { AppPage.RequestsFromOthers,  typeof(RequestsFromOthersPage) },
            { AppPage.RentalsFromOthers,   typeof(RentalsFromOthersPage) },
            { AppPage.RequestsToOthers,    typeof(RequestsToOthersPage) },
            { AppPage.RentalsToOthers,     typeof(RentalsToOthersPage) },
            { AppPage.Notifications,       typeof(NotificationsPage) },
            { AppPage.Profile,             typeof(ProfilePage) },
            { AppPage.Admin,               typeof(AdminPage) },
        };

        private IGameService injectedGameService;

        public MenuBarPage()
        {
            this.InitializeComponent();
            this.ViewModel = App.Services.GetRequiredService<MenuBarViewModel>();
            this.DataContext = this.ViewModel;
            this.ViewModel.RequestNavigation += this.OnViewModelRequestedNavigation;
            this.Unloaded += this.OnMenuBarPageUnloaded;
        }

        public MenuBarViewModel ViewModel { get; }

        public void NavigateToNotifications()
        {
            var resolvedNotificationsViewModel = App.Services.GetRequiredService<NotificationsViewModel>();
            this.ContentFrame.Navigate(typeof(NotificationsPage), resolvedNotificationsViewModel);
            this.ViewModel.SelectedPageName = "Notifications";
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is IGameService gameService)
            {
                this.injectedGameService = gameService;
            }

            if (this.ContentFrame.Content == null)
            {
                this.ContentFrame.Navigate(typeof(ListingsPage), this.injectedGameService);
            }
        }

        private void OnViewModelRequestedNavigation(AppPage page)
        {
            if (page == AppPage.Logout)
            {
                App.OnUserLoggedOut();
                return;
            }

            if (!PageTypeMap.TryGetValue(page, out var pageType))
            {
                return;
            }

            this.ContentFrame.Navigate(pageType, this.injectedGameService);
        }

        private void OnMenuBarPageUnloaded(object pageSender, RoutedEventArgs unloadedEventArgs)
        {
            this.ViewModel.RequestNavigation -= this.OnViewModelRequestedNavigation;
        }
    }
}
