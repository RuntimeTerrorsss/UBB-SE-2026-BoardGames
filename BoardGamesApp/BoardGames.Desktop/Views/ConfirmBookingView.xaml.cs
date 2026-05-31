// <copyright file="ConfirmBookingView.xaml.cs" company="BoardRent">
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
    public sealed partial class ConfirmBookingView : Page
    {
        private ConfirmBookingViewModel? viewModel;

        public ConfirmBookingView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            if (eventArgs.Parameter is not RentalConfirmNavigation navigation)
            {
                App.NavigateBack();
                return;
            }

            viewModel = new ConfirmBookingViewModel(
                App.Services.GetRequiredService<IRequestService>(),
                App.Services.GetRequiredService<ISessionContext>());

            viewModel.Initialize(navigation.Game, navigation.StartDate, navigation.EndDate);

            viewModel.OnGoBackRequested = () =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    App.NavigateTo(AppPage.GameDetails, navigation.Game.Id);
                }
            };

            viewModel.OnConfirmBookingRequested = () =>
            {
                App.NavigateTo(AppPage.Chat, clearBackStack: false);
            };

            viewModel.OnErrorOccurred += async message =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot,
                };
                await dialog.ShowAsync();
            };

            DataContext = viewModel;
        }

        private void OnBackClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs eventArgs)
        {
            viewModel?.GoBack();
        }

        private async void OnConfirmClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs eventArgs)
        {
            if (viewModel is null)
            {
                return;
            }

            await viewModel.ConfirmBookingAsync();
        }
    }
}
