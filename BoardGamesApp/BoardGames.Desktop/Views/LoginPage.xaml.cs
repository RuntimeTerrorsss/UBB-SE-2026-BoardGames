// <copyright file="LoginPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<LoginViewModel>();
            this.DataContext = this.ViewModel;

            this.InitializeNavigationCallbacks();
        }

        public LoginViewModel ViewModel { get; }

        private void InitializeNavigationCallbacks()
        {
            this.ViewModel.OnLoginSuccess = () =>
            {
                App.OnUserLoggedIn();
            };

            this.ViewModel.OnNavigateToRegister = () =>
            {
                App.NavigateTo(AppPage.Register);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            this.ViewModel.InfoMessage = navigationEventArgs.Parameter as string ?? string.Empty;
            this.PasswordBox.Password = string.Empty;
            this.ViewModel.Password = string.Empty;
        }

        private async void ForgotPassword_Click(object pointerSender, RoutedEventArgs eventArgs)
        {
            await this.ResetPasswordDialog.ShowAsync();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs eventArgs)
        {
            this.ViewModel.Password = this.PasswordBox.Password;
        }
    }
}
