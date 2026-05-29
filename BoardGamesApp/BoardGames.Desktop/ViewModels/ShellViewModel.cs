using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using BoardGames.Desktop.Services;

namespace BoardGames.Desktop.ViewModels
{
    public sealed partial class ShellViewModel : ObservableObject
    {
        private readonly IDesktopAuthorizationService authorizationService;

        public ShellViewModel(IDesktopAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            Refresh();
        }

        public ObservableCollection<ShellNavigationItem> NavigationItems { get; } = new();

        [ObservableProperty]
        private ShellNavigationItem? selectedItem;

        public AppPage CurrentRoute { get; private set; } = AppPage.Filter;

        public void Refresh()
        {
            NavigationItems.Clear();
            AddItem(AppPage.Filter, "Search Games");

            if (!authorizationService.IsLoggedIn)
            {
                AddItem(AppPage.Login, "Login");
                AddItem(AppPage.Register, "Register");
            }
            else
            {
                AddItem(AppPage.Games, "Games");
                AddItem(AppPage.Notifications, "Notifications");
                AddItem(AppPage.Dashboard, "Dashboard");
                AddItem(AppPage.Chat, "Chat");
                AddItem(AppPage.Account, "Account");

                if (authorizationService.IsAdministrator)
                {
                    AddItem(AppPage.Admin, "Admin");
                }

                AddItem(AppPage.Logout, "Logout");
            }

            SelectedItem = FindItem(CurrentRoute) ?? NavigationItems.FirstOrDefault();
        }

        public void SetCurrentRoute(AppPage route)
        {
            CurrentRoute = route;
            SelectedItem = FindItem(route);
        }

        public ShellNavigationItem? FindItem(AppPage route)
        {
            return NavigationItems.FirstOrDefault(item => item.Route == route);
        }

        private void AddItem(AppPage route, string label)
        {
            NavigationItems.Add(new ShellNavigationItem(route, label));
        }
    }

    public sealed record ShellNavigationItem(AppPage Route, string Label);
}
