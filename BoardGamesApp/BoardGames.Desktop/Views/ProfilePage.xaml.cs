// <copyright file="ProfilePage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.Extensions.DependencyInjection;

    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<ProfileViewModel>();
            this.DataContext = this.ViewModel;

            this.ViewModel.OnSignOutSuccess = () =>
            {
                App.OnUserLoggedOut();
            };

            this.Loaded += async (object sender, RoutedEventArgs eventArguments) =>
            {
                await this.ViewModel.LoadProfileAsync();
            };
        }

        public ProfileViewModel ViewModel { get; }
    }
}
