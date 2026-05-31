using System;
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Services
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
