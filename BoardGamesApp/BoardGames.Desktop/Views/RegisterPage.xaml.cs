// <copyright file="RegisterPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<RegisterViewModel>();
            this.DataContext = this.ViewModel;

            this.InitializeNavigationCallbacks();
        }

        public RegisterViewModel ViewModel { get; }

        private void InitializeNavigationCallbacks()
        {
            this.ViewModel.OnRegistrationSuccess = successMessage =>
            {
                App.NavigateTo(AppPage.Login, successMessage);
            };

            this.ViewModel.OnNavigateToLogin = () =>
            {
                App.NavigateTo(AppPage.Login);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            this.PasswordBox.Password = string.Empty;
            this.ConfirmPasswordBox.Password = string.Empty;
            this.ViewModel.Password = string.Empty;
            this.ViewModel.ConfirmPassword = string.Empty;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs eventArgs)
        {
            this.ViewModel.Password = this.PasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs eventArgs)
        {
            this.ViewModel.ConfirmPassword = this.ConfirmPasswordBox.Password;
        }
    }
}
