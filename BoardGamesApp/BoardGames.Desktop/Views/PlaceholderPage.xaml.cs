// <copyright file="PlaceholderPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class PlaceholderPage : Page
    {
        public PlaceholderPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            var model = navigationEventArgs.Parameter as PlaceholderPageModel
                ?? new PlaceholderPageModel("Feature", "This feature will be wired in a later task.");

            TitleText.Text = model.Title;
            DescriptionText.Text = model.Description;
        }
    }

    public sealed record PlaceholderPageModel(string Title, string Description);
}
