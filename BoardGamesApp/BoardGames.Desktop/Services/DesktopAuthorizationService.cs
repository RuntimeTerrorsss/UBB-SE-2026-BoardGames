// <copyright file="DesktopAuthorizationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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

        public Guid CurrentAccountId => this.sessionContext.AccountId;

        public bool IsLoggedIn => this.sessionContext.IsLoggedIn;

        public bool IsAdministrator =>
            string.Equals(this.sessionContext.Role, AppRoles.Administrator, StringComparison.Ordinal);

        public bool CanAccessPage(Type pageType)
        {
            return pageType == typeof(ShellPage)
                || pageType == typeof(SearchGamesPage)
                || pageType == typeof(GameDetailsPage)
                || pageType == typeof(ConfirmBookingView)
                || pageType == typeof(LoginPage)
                || pageType == typeof(RegisterPage)
                || (pageType == typeof(PlaceholderPage) && this.IsLoggedIn);
        }

        public bool CanAccessRoute(AppPage page)
        {
            if (page is AppPage.Filter or AppPage.GameDetails or AppPage.Login or AppPage.Register)
            {
                return true;
            }

            if (page == AppPage.ConfirmRental)
            {
                return this.IsLoggedIn;
            }

            if (!this.IsLoggedIn)
            {
                return false;
            }

            return page != AppPage.Admin || this.IsAdministrator;
        }
    }
}
