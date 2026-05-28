using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Views.ChatViews;

namespace BoardGames.Desktop.Views
{
    public sealed partial class MenuBarPage : Page
    {
        private static readonly Dictionary<AppPage, Type> PageTypeMap = new()
        {
            { AppPage.Dashboard,           typeof(DashboardView) },
            { AppPage.Chat,                typeof(ChatPageView) },
            { AppPage.Listings,            typeof(ListingsPage) },
            { AppPage.Notifications,       typeof(NotificationsPage) },
            { AppPage.Profile,             typeof(ProfilePage) },
            { AppPage.Admin,               typeof(AdminPage) },
        };

        private readonly IDesktopAuthorizationService authorizationService;

        public MenuBarPage()
        {
            this.InitializeComponent();
            this.ViewModel = App.Services.GetRequiredService<MenuBarViewModel>();
            this.authorizationService = App.Services.GetRequiredService<IDesktopAuthorizationService>();
            this.DataContext = this.ViewModel;
            this.ViewModel.RequestNavigation += this.OnViewModelRequestedNavigation;
            this.ContentFrame.Navigating += this.OnContentFrameNavigating;
            this.Unloaded += this.OnMenuBarPageUnloaded;
        }

        public MenuBarViewModel ViewModel { get; }

        private void PerformLogout()
        {
            this.Frame?.Navigate(typeof(LoginPage));
        }

        public void NavigateToNotifications()
        {
            if (!this.authorizationService.CanAccessPage(typeof(NotificationsPage)))
            {
                if (!this.authorizationService.IsLoggedIn)
                {
                    PerformLogout();
                }

                return;
            }

            var resolvedNotificationsViewModel = App.Services.GetRequiredService<NotificationsViewModel>();
            this.ContentFrame.Navigate(typeof(NotificationsPage), resolvedNotificationsViewModel);
            this.ViewModel.SelectedPageName = "Notifications";
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (!this.authorizationService.IsLoggedIn)
            {
                PerformLogout();
                return;
            }

            if (this.ContentFrame.Content == null)
            {
                this.ContentFrame.Navigate(typeof(ListingsPage));
            }
        }

        private void OnViewModelRequestedNavigation(AppPage page)
        {
            if (page == AppPage.Logout)
            {
                PerformLogout();
                return;
            }

            if (!this.authorizationService.CanAccessMenuPage(page))
            {
                if (!this.authorizationService.IsLoggedIn)
                {
                    PerformLogout();
                }

                return;
            }

            if (!PageTypeMap.TryGetValue(page, out var pageType))
            {
                return;
            }

            this.ContentFrame.Navigate(pageType);
        }

        private void OnContentFrameNavigating(object sender, NavigatingCancelEventArgs navigatingEventArgs)
        {
            if (navigatingEventArgs.SourcePageType == null
                || this.authorizationService.CanAccessPage(navigatingEventArgs.SourcePageType))
            {
                return;
            }

            navigatingEventArgs.Cancel = true;

            if (!this.authorizationService.IsLoggedIn)
            {
                PerformLogout();
                return;
            }

            if (this.ContentFrame.Content == null)
            {
                this.ContentFrame.Navigate(typeof(ListingsPage));
            }
        }

        private void OnMenuBarPageUnloaded(object pageSender, RoutedEventArgs unloadedEventArgs)
        {
            this.ViewModel.RequestNavigation -= this.OnViewModelRequestedNavigation;
            this.ContentFrame.Navigating -= this.OnContentFrameNavigating;
        }
    }
}