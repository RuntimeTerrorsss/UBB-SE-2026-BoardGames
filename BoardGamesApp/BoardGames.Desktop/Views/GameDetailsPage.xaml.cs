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
            CurrentDate = DateTimeOffset.Now.Date;
        }

        public DateTimeOffset CurrentDate { get; }

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

            ViewModel.OnRequestSuccess = () => App.NavigateTo(AppPage.Chat, clearBackStack: true);

            DataContext = ViewModel;
            await ViewModel.LoadAsync(gameId);
        }
    }
}
