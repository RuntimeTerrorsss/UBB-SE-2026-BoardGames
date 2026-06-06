// <copyright file="ShellPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BoardGames.Desktop.Views
{
    public sealed partial class ShellPage : Page
    {
        private readonly IDesktopAuthorizationService authorizationService;
        private bool isInternalSelectionUpdate;

        public ShellPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<ShellViewModel>();
            authorizationService = App.Services.GetRequiredService<IDesktopAuthorizationService>();
            DataContext = ViewModel;
            Loaded += OnLoaded;
        }

        public ShellViewModel ViewModel { get; }

        public void RefreshNavigation()
        {
            ViewModel.Refresh();
            NavigationList.SelectedItem = ViewModel.FindItem(ViewModel.CurrentRoute);

            if (!authorizationService.CanAccessRoute(ViewModel.CurrentRoute))
            {
                NavigateTo(AppPage.Filter, clearBackStack: true);
            }
        }

        public void NavigateTo(AppPage route, object? parameter = null, bool clearBackStack = false)
        {
            if (route == AppPage.Logout)
            {
                _ = LogoutAsync();
                return;
            }

            if (!authorizationService.CanAccessRoute(route))
            {
                if (!authorizationService.IsLoggedIn)
                {
                    App.OnUserLoggedOut();
                }

                return;
            }

            var (pageType, navigationParameter) = ResolveNavigation(route, parameter);

            ContentFrame.Navigate(pageType, navigationParameter);
            if (clearBackStack)
            {
                ContentFrame.BackStack.Clear();
            }

            ViewModel.SetCurrentRoute(route);
            isInternalSelectionUpdate = true;
            NavigationList.SelectedItem = ViewModel.FindItem(route);
            isInternalSelectionUpdate = false;
        }

        public void NavigateBack()
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            RefreshNavigation();

            if (ContentFrame.Content == null)
            {
                NavigateTo(AppPage.Filter, clearBackStack: true);
            }
        }

        private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            if (isInternalSelectionUpdate || NavigationList.SelectedItem is not ShellNavigationItem selectedItem)
            {
                return;
            }

            NavigateTo(selectedItem.Route);
        }

        private static (Type PageType, object? Parameter) ResolveNavigation(AppPage route, object? parameter)
        {
            return route switch
            {
                AppPage.Filter => (typeof(SearchGamesPage), parameter),
                AppPage.GameDetails => (typeof(GameDetailsPage), parameter),
                AppPage.ConfirmRental => (typeof(ConfirmBookingView), parameter),
                AppPage.Login => (typeof(LoginPage), parameter),
                AppPage.Register => (typeof(RegisterPage), parameter),
                AppPage.Games => (typeof(ListingsPage), parameter),
                AppPage.Notifications => (typeof(NotificationsPage), parameter),
                AppPage.Dashboard => (typeof(DashboardView), parameter),
                AppPage.Chat => (typeof(ChatPage), parameter),
                AppPage.Account => (typeof(ProfilePage), parameter),
                AppPage.Admin => (typeof(AdminPage), parameter),
                _ => (typeof(SearchGamesPage), parameter),
            };
        }

        private async Task LogoutAsync()
        {
            var authService = App.Services.GetRequiredService<IAuthService>();

            try
            {
                await authService.LogoutAsync();
            }
            finally
            {
                App.OnUserLoggedOut();
            }
        }
    }
}
