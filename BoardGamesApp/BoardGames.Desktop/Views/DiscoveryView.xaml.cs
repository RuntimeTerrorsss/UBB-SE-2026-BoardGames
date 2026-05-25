// <copyright file="DiscoveryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Src.Helpers;
using BookingBoardGames.Src.ViewModels;
using BookingBoardGames.Src.Views.ChatViews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardGames.Src.Views
{
    /// <summary>
    /// Provides the main discovery interface for browsing and filtering available games.
    /// </summary>
    public sealed partial class DiscoveryView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryView"/> class.
        /// </summary>
        public DiscoveryView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with the discovery logic.
        /// </summary>
        public DiscoveryViewModel ViewModel { get; private set; } = null!;

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="navigationArgs">Event data that can be examined by overriding code.</param>
        protected override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);

            this.UpdateAuthUi();

            this.ViewModel = new DiscoveryViewModel(App.SearchAndFilterService, App.GlobalGeographicalService);

            this.ViewModel.OnSearchRequest += this.HandleSearchRequest;
            this.ViewModel.OnGameSelectedRequest += gameId =>
            {
                if (!this.EnsureLoggedIn())
                {
                    return;
                }

                this.Frame.Navigate(typeof(GameDetailsView), gameId);
            };

            this.ViewModel.OnPageChanged += () =>
            {
                this.MainScrollViewer.ScrollToVerticalOffset(0);
            };

            this.DataContext = this.ViewModel;
            this.StartDatePicker.Date = this.ViewModel.SelectedStartDate;
            this.EndDatePicker.Date = this.ViewModel.SelectedEndDate;
        }

        private void UpdateAuthUi()
        {
            bool isLoggedIn = AuthSession.IsLoggedIn;
            this.LoginButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            this.DashboardButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            this.ChatButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            this.LogoutButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HandleSearchRequest(FilterCriteria filter)
        {
            this.Frame.Navigate(typeof(FilteredSearchView), filter);
        }

        private void Game_Click(object sender, ItemClickEventArgs itemClickedArgs)
        {
            if (!this.EnsureLoggedIn())
            {
                return;
            }

            if (itemClickedArgs.ClickedItem is GameDTO game)
            {
                this.Frame.Navigate(typeof(GameDetailsView), game.GameId);
            }
        }

        private bool EnsureLoggedIn()
        {
            if (AuthSession.IsLoggedIn)
            {
                return true;
            }

            _ = this.ShowLoginRequiredDialogAsync();
            return false;
        }

        private async System.Threading.Tasks.Task ShowLoginRequiredDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Login required",
                Content = "You need to log in to book a game.",
                PrimaryButtonText = "Login",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(LoginView));
            }
        }

        private void EndDatePicker_DayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            if (this.ViewModel?.SelectedStartDate.HasValue == true)
            {
                var date = args.Item.Date.Date;
                var selectedStartDate = this.ViewModel.SelectedStartDate.Value.Date;

                if (date < selectedStartDate)
                {
                    args.Item.IsBlackout = true;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            this.Frame.Navigate(typeof(LoginView));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            AuthSession.Clear(App.Session);
            this.UpdateAuthUi();
            this.Frame.Navigate(typeof(LoginView));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(ChatPageView), SessionContext.GetInstance().UserId);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(DashboardView), SessionContext.GetInstance().UserId);
        }
    }
}
