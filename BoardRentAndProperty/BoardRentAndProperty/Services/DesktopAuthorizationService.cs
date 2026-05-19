using System;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using BoardRentAndProperty.Views;

namespace BoardRentAndProperty.Services
{
    public class DesktopAuthorizationService : IDesktopAuthorizationService
    {
        private readonly ISessionContext sessionContext;

        public DesktopAuthorizationService(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
        }

        public Guid CurrentAccountId => this.sessionContext.AccountId;

        public bool IsLoggedIn => this.sessionContext.IsLoggedIn;

        public bool IsAdministrator =>
            string.Equals(this.sessionContext.Role, AppRoles.Administrator, StringComparison.Ordinal);

        public bool CanAccessPage(Type pageType)
        {
            if (this.IsPublicPage(pageType))
            {
                return true;
            }

            if (!this.IsLoggedIn)
            {
                return false;
            }

            return pageType != typeof(AdminPage) || this.IsAdministrator;
        }

        public bool CanAccessMenuPage(AppPage page)
        {
            if (!this.IsLoggedIn)
            {
                return false;
            }

            return page != AppPage.Admin || this.IsAdministrator;
        }

        private bool IsPublicPage(Type pageType) =>
            pageType == typeof(LoginPage) || pageType == typeof(RegisterPage);
    }
}
