// <copyright file="GameDetailsPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class GameDetailsPage : Page
    {
        public GameDetailsPage()
        {
            InitializeComponent();
        }

        public GameDetailsPageViewModel? ViewModel { get; private set; }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is not int gameId)
            {
                App.NavigateTo(AppPage.Filter, clearBackStack: true);
                return;
            }

            ViewModel = new GameDetailsPageViewModel(
                App.Services.GetRequiredService<IGameService>(),
                App.Services.GetRequiredService<IRequestService>(),
                App.Services.GetRequiredService<IRentalService>(),
                App.Services.GetRequiredService<ISessionContext>());

            ViewModel.OnBackRequested = () =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    App.NavigateTo(AppPage.Filter, clearBackStack: true);
                }
            };

            ViewModel.OnLoginRequested = message => App.NavigateTo(AppPage.Login, message);

            ViewModel.OnNavigateToConfirm = (game, start, end) =>
                App.NavigateTo(AppPage.ConfirmRental, new RentalConfirmNavigation(game, start, end));

            ViewModel.OnCalendarRangesLoaded = RefreshCalendarRanges;

            RentalCalendar.SelectionChanged += OnRentalCalendarSelectionChanged;

            DataContext = ViewModel;
            await ViewModel.LoadAsync(gameId);
            RefreshCalendarRanges();
        }

        private void OnRentalCalendarSelectionChanged(object? sender, EventArgs e)
        {
            if (ViewModel is null)
            {
                return;
            }

            ViewModel.StartDate = RentalCalendar.StartDate.HasValue
                ? new DateTimeOffset(RentalCalendar.StartDate.Value)
                : null;
            ViewModel.EndDate = RentalCalendar.EndDate.HasValue
                ? new DateTimeOffset(RentalCalendar.EndDate.Value)
                : null;
        }

        private void RefreshCalendarRanges()
        {
            if (ViewModel is null)
            {
                return;
            }

            RentalCalendar.SetBookedRanges(ViewModel.BookedRanges);
            RentalCalendar.SetPendingRequestRanges(ViewModel.PendingRequestRanges);
        }
    }
}
