// <copyright file="DialogHelper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Views
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using AppConstants = BoardGames.Desktop.Constants.Constants;

    internal static class DialogHelper
    {
        public static async Task ShowMessageAsync(XamlRoot xamlRoot, string title, object content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = AppConstants.DialogButtons.Ok,
                XamlRoot = xamlRoot,
            };

            await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> ShowConfirmationAsync(
            XamlRoot xamlRoot,
            string title,
            object content,
            string primaryButtonText,
            string closeButtonText,
            ContentDialogButton defaultButton)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = defaultButton,
                XamlRoot = xamlRoot,
            };

            return await dialog.ShowAsync();
        }
    }
}
