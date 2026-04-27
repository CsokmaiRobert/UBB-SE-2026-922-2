using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.ViewModels;

namespace BoardRentAndProperty.Views
{
    public sealed partial class MenuBarPage : Page
    {
        public MenuBarViewModel ViewModel { get; }

        private static readonly Dictionary<AppPage, Type> PageTypeMap = new()
        {
            { AppPage.Listings,            typeof(ListingsPage) },
            { AppPage.RequestsFromOthers,  typeof(RequestsFromOthersPage) },
            { AppPage.RentalsFromOthers,   typeof(RentalsFromOthersPage) },
            { AppPage.RequestsToOthers,    typeof(RequestsToOthersPage) },
            { AppPage.RentalsToOthers,     typeof(RentalsToOthersPage) },
            { AppPage.Notifications,       typeof(NotificationsPage) }
        };

        private IGameService injectedGameService;

        public MenuBarPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<MenuBarViewModel>();
            this.DataContext = ViewModel;
            ViewModel.RequestNavigation += OnViewModelRequestedNavigation;
            this.Unloaded += OnMenuBarPageUnloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is IGameService gameService)
            {
                injectedGameService = gameService;
            }

            // After returning from BoardRent area, reset to My Games to avoid the singleton VM
            // showing "BoardRent" as selected with no content rendered.
            if (ViewModel.SelectedPageName == "BoardRent" || string.IsNullOrEmpty(ViewModel.SelectedPageName))
            {
                ViewModel.SelectedPageName = "My Games";
                ContentFrame.Navigate(typeof(ListingsPage), injectedGameService);
            }
        }

        private void OnViewModelRequestedNavigation(AppPage page)
        {
            if (page == AppPage.BoardRent)
            {
                App.NavigateTo(typeof(LoginPage));
                return;
            }

            if (!PageTypeMap.TryGetValue(page, out var pageType))
            {
                return;
            }

            ContentFrame.Navigate(pageType, injectedGameService);
        }

        private void OnMenuBarPageUnloaded(object pageSender, RoutedEventArgs unloadedEventArgs)
        {
            ViewModel.RequestNavigation -= OnViewModelRequestedNavigation;
        }

        public void NavigateToNotifications()
        {
            var resolvedNotificationsViewModel = App.Services.GetRequiredService<NotificationsViewModel>();
            ContentFrame.Navigate(typeof(NotificationsPage), resolvedNotificationsViewModel);
            ViewModel.SelectedPageName = "Notifications";
        }
    }
}
