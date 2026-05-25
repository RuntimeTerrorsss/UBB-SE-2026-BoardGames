namespace BoardGames.Desktop.ViewModels
{
    using BoardGames.Desktop.Services;

    public class MenuBarViewModel : BaseViewModel
    {
        private const string DefaultSelectedMenuLabel = "My Games";

        private readonly IDesktopAuthorizationService authorizationService;

        private Dictionary<string, Action> navigationActionsByMenuLabel;
        private string selectedMenuPageName;

        public MenuBarViewModel(IDesktopAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            navigationActionsByMenuLabel = BuildNavigationActions();
        }

        public MenuBarViewModel(BoardRentAndProperty.Utilities.ISessionContext sessionContext)
            : this(new DesktopAuthorizationService(sessionContext))
        {
        }

        public event Action<AppPage> RequestNavigation;

        public Dictionary<string, Action> NavigationActionsByMenuLabel
        {
            get => navigationActionsByMenuLabel;
            private set
            {
                navigationActionsByMenuLabel = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedPageName
        {
            get => selectedMenuPageName;
            set
            {
                if (selectedMenuPageName != value)
                {
                    selectedMenuPageName = value;
                    this.OnPropertyChanged();
                    HandleMenuNavigation(value);
                }
            }
        }

        public void Rebuild()
        {
            NavigationActionsByMenuLabel = BuildNavigationActions();
            selectedMenuPageName = DefaultSelectedMenuLabel;
            this.OnPropertyChanged(nameof(SelectedPageName));
        }

        private Dictionary<string, Action> BuildNavigationActions()
        {
            var actions = new Dictionary<string, Action>
            {
                { "My Games",         () => RequestNavigation?.Invoke(AppPage.Listings) },
                { "My Requests",      () => RequestNavigation?.Invoke(AppPage.RequestsToOthers) },
                { "My Rentals",       () => RequestNavigation?.Invoke(AppPage.RentalsFromOthers) },
                { "Others' Requests", () => RequestNavigation?.Invoke(AppPage.RequestsFromOthers) },
                { "Others' Rentals",  () => RequestNavigation?.Invoke(AppPage.RentalsToOthers) },
                { "Notifications",    () => RequestNavigation?.Invoke(AppPage.Notifications) },
                { "Profile",          () => RequestNavigation?.Invoke(AppPage.Profile) },
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
            if (!string.IsNullOrEmpty(selectedMenuLabel)
                && navigationActionsByMenuLabel.TryGetValue(selectedMenuLabel, out var navigationAction))
            {
                navigationAction.Invoke();
            }
        }
    }
}
