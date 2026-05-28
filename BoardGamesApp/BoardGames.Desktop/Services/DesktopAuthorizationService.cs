using BoardGames.Desktop.ViewModels;
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
            return pageType == typeof(ShellPage)
                || pageType == typeof(SearchGamesPage)
                || pageType == typeof(LoginPage)
                || pageType == typeof(RegisterPage)
                || (pageType == typeof(PlaceholderPage) && IsLoggedIn);
        }

        public bool CanAccessRoute(AppPage page)
        {
            if (page is AppPage.Filter or AppPage.Login or AppPage.Register)
            {
                return true;
            }

            if (!IsLoggedIn)
            {
                return false;
            }

            return page != AppPage.Admin || IsAdministrator;
        }
    }
}
