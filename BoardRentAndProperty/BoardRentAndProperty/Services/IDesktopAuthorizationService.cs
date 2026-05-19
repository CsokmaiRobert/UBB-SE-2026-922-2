using System;
using BoardRentAndProperty.ViewModels;

namespace BoardRentAndProperty.Services
{
    public interface IDesktopAuthorizationService
    {
        Guid CurrentAccountId { get; }

        bool IsLoggedIn { get; }

        bool IsAdministrator { get; }

        bool CanAccessPage(Type pageType);

        bool CanAccessMenuPage(AppPage page);
    }
}
