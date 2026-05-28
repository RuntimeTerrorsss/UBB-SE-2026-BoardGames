namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using BoardGames.Desktop.Services;

    public class MenuBarViewModel : BaseViewModel
    {
        private const string DefaultSelectedMenuLabel = "Dashboard";

        private readonly IDesktopAuthorizationService authorizationService;
        private Dictionary<string, Action> navigationActionsByMenuLabel;
        private string selectedMenuPageName = DefaultSelectedMenuLabel;

        public MenuBarViewModel(IDesktopAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            navigationActionsByMenuLabel = BuildNavigationActions();
        }

        public MenuBarViewModel(ISessionContext sessionContext)
            : this(new DesktopAuthorizationService(sessionContext))
        {
        }

        public event Action<AppPage>? RequestNavigation;

        public Dictionary<string, Action> NavigationActionsByMenuLabel
        {
            get => navigationActionsByMenuLabel;
            private set => SetProperty(ref navigationActionsByMenuLabel, value);
        }

        public string SelectedPageName
        {
            get => selectedMenuPageName;
            set
            {
                if (SetProperty(ref selectedMenuPageName, value))
                {
                    HandleMenuNavigation(value);
                }
            }
        }

        public void Rebuild()
        {
            NavigationActionsByMenuLabel = BuildNavigationActions();
            SelectedPageName = DefaultSelectedMenuLabel;
        }

        private Dictionary<string, Action> BuildNavigationActions()
        {
            var actions = new Dictionary<string, Action>
            {
                { "Dashboard",     () => RequestNavigation?.Invoke(AppPage.Dashboard) },
                { "Chat",          () => RequestNavigation?.Invoke(AppPage.Chat) },
                { "My Games",      () => RequestNavigation?.Invoke(AppPage.Listings) },
                { "Notifications", () => RequestNavigation?.Invoke(AppPage.Notifications) },
                { "Profile",       () => RequestNavigation?.Invoke(AppPage.Profile) },
            };

            if (authorizationService.IsAdministrator)
            {
                actions.Add("Admin", () => RequestNavigation?.Invoke(AppPage.Admin));
            }

            actions.Add("Logout", () => RequestNavigation?.Invoke(AppPage.Logout));

            return actions;
        }

        private void HandleMenuNavigation(string selectedMenuLabel)
        {
            if (navigationActionsByMenuLabel.TryGetValue(selectedMenuLabel, out var navigationAction))
            {
                navigationAction.Invoke();
            }
        }
    }
}