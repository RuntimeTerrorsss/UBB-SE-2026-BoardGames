// <copyright file="ListingsPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class ListingsPage : Page
    {
        public ListingsViewModel ViewModel { get; private set; }

        public ListingsPage()
        {
            this.InitializeComponent();
        }

        private void FilterMyGamesButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel?.ToggleMyGamesFilter();
        }

        private void CreateGameButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Frame.Navigate(typeof(CreateGameView));
        }

        private void EditGameButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if ((sender as Button)?.Tag is GameSummaryDTO gameToEdit)
            {
                this.Frame.Navigate(typeof(EditGameView), gameToEdit.Id);
            }
        }

        private async void DeleteGameButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var clickedButton = sender as Button;
            var gameToDelete = clickedButton?.Tag as GameSummaryDTO;

            if (gameToDelete == null)
            {
                return;
            }

            var confirmDialog = new ContentDialog
            {
                Title = "Delete Game?",
                Content = $"Are you sure you want to permanently delete '{gameToDelete.Name}'? Pending requests will be cancelled.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot,
            };

            var deleteConfirmationResult = await confirmDialog.ShowAsync();

            if (deleteConfirmationResult == ContentDialogResult.Primary)
            {
                var gameDeletionResult = await ViewModel.TryDeleteGameAsync(gameToDelete);
                if (!string.IsNullOrWhiteSpace(gameDeletionResult.DialogMessage))
                {
                    var msgDialog = new ContentDialog
                    {
                        Title = gameDeletionResult.DialogTitle ?? "Notice",
                        Content = gameDeletionResult.DialogMessage,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot,
                    };
                    await msgDialog.ShowAsync();
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            ViewModel = App.Services.GetRequiredService<ListingsViewModel>();
            this.DataContext = ViewModel;
            await ViewModel.LoadGamesAsync();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel?.PrevPage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel?.NextPage();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Image failedImage)
            {
                if (this.Resources.TryGetValue("DefaultGameImage", out var defaultImage))
                {
                    failedImage.Source = defaultImage as Microsoft.UI.Xaml.Media.ImageSource;
                }
                else
                {
                    failedImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/default-game-placeholder.jpg"));
                }
            }
        }
    }
}
