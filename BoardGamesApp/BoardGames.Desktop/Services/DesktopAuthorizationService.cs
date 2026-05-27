using System;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.Views;

namespace BoardGames.Desktop.Services
{
    public class DesktopAuthorizationService : IDesktopAuthorizationService
    {
        private readonly ISessionContext sessionContext;

        public DesktopAuthorizationService(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
        }

        public Guid CurrentAccountId => sessionContext.AccountId;

        public bool IsLoggedIn => sessionContext.IsLoggedIn;

        public bool IsAdministrator =>
            string.Equals(sessionContext.Role, AppRoles.Administrator, StringComparison.Ordinal);

        public bool CanAccessPage(Type pageType)
        {
            if (IsPublicPage(pageType))
            {
                return true;
            }

            if (!IsLoggedIn)
            {
                return false;
            }

            return pageType != typeof(AdminPage) || IsAdministrator;
        }

        public bool CanAccessMenuPage(AppPage page)
        {
            if (!IsLoggedIn)
            {
                return false;
            }

            return page != AppPage.Admin || IsAdministrator;
        }

        private bool IsPublicPage(Type pageType) =>
            pageType == typeof(LoginPage) || pageType == typeof(RegisterPage);
    }
}
