namespace BoardRentAndProperty.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using BoardRentAndProperty.Utilities;

    public class MenuBarViewModel : INotifyPropertyChanged
    {
        private const string AdministratorRoleName = "Administrator";
        private const string DefaultSelectedMenuLabel = "My Games";

        private readonly ISessionContext sessionContext;

        private Dictionary<string, Action> navigationActionsByMenuLabel;
        private string selectedMenuPageName;

        public MenuBarViewModel(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
            this.navigationActionsByMenuLabel = this.BuildNavigationActions();
        }

        public event Action<AppPage> RequestNavigation;

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<string, Action> NavigationActionsByMenuLabel
        {
            get => this.navigationActionsByMenuLabel;
            private set
            {
                this.navigationActionsByMenuLabel = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedPageName
        {
            get => this.selectedMenuPageName;
            set
            {
                if (this.selectedMenuPageName != value)
                {
                    this.selectedMenuPageName = value;
                    this.OnPropertyChanged();
                    this.HandleMenuNavigation(value);
                }
            }
        }

        public void Rebuild()
        {
            this.NavigationActionsByMenuLabel = this.BuildNavigationActions();
            this.selectedMenuPageName = DefaultSelectedMenuLabel;
            this.OnPropertyChanged(nameof(this.SelectedPageName));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Dictionary<string, Action> BuildNavigationActions()
        {
            var actions = new Dictionary<string, Action>
            {
                { "My Games",         () => this.RequestNavigation?.Invoke(AppPage.Listings) },
                { "My Requests",      () => this.RequestNavigation?.Invoke(AppPage.RequestsToOthers) },
                { "My Rentals",       () => this.RequestNavigation?.Invoke(AppPage.RentalsFromOthers) },
                { "Others' Requests", () => this.RequestNavigation?.Invoke(AppPage.RequestsFromOthers) },
                { "Others' Rentals",  () => this.RequestNavigation?.Invoke(AppPage.RentalsToOthers) },
                { "Notifications",    () => this.RequestNavigation?.Invoke(AppPage.Notifications) },
                { "Profile",          () => this.RequestNavigation?.Invoke(AppPage.Profile) },
            };

            if (this.sessionContext.Role == AdministratorRoleName)
            {
                actions.Add("Admin", () => this.RequestNavigation?.Invoke(AppPage.Admin));
            }

            actions.Add("Logout", () => this.RequestNavigation?.Invoke(AppPage.Logout));

            return actions;
        }

        private void HandleMenuNavigation(string selectedMenuLabel)
        {
            if (!string.IsNullOrEmpty(selectedMenuLabel)
                && this.navigationActionsByMenuLabel.TryGetValue(selectedMenuLabel, out var navigationAction))
            {
                navigationAction.Invoke();
            }
        }
    }
}
