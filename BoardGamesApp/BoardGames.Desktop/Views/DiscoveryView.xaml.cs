// <copyright file="DiscoveryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.ViewModels;
using BoardGames.Data.Enums;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
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

        public static int loggedUserId = MainWindow.loggedInUserAlice;

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="navigationArgs">Event data that can be examined by overriding code.</param>
        protected override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);

            // Static loggedUserId survives navigation; re-apply session and refresh the switch label
            // (otherwise the button resets to the XAML default and lies about who's active).
            SessionContext.GetInstance().UserId = loggedUserId;
            this.SyncSwitchUserButtonLabel();

            this.ViewModel = new DiscoveryViewModel(App.SearchAndFilterService, App.GlobalGeographicalService);

            this.ViewModel.OnSearchRequest += this.HandleSearchRequest;
            this.ViewModel.OnGameSelectedRequest += gameId =>
            {
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

        private void HandleSearchRequest(FilterCriteria filter)
        {
            this.Frame.Navigate(typeof(FilteredSearchView), filter);
        }

        private void Game_Click(object sender, ItemClickEventArgs itemClickedArgs)
        {
            if (itemClickedArgs.ClickedItem is GameDTO game)
            {
                this.Frame.Navigate(typeof(GameDetailsView), game.GameId);
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

        private void ChatButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            this.Frame.Navigate(typeof(ChatPageView), SessionContext.GetInstance().UserId);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            var app = (App)Application.Current;
            this.Frame.Navigate(typeof(DashboardView), SessionContext.GetInstance().UserId);
        }

        private void SyncSwitchUserButtonLabel()
        {
            this.SwitchUserButton.Content = loggedUserId == MainWindow.loggedInUserAlice
                ? "Switch to Carol"
                : "Switch to Bob";
        }

        private void SwitchUserButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (loggedUserId == MainWindow.loggedInUserAlice)
            {
                loggedUserId = MainWindow.loggedInUserBob;
            }
            else
            {
                loggedUserId = MainWindow.loggedInUserAlice;
            }

            SessionContext.GetInstance().UserId = loggedUserId;
            this.SyncSwitchUserButtonLabel();
        }
    }
}
