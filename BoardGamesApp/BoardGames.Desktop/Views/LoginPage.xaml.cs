namespace BoardGames.Desktop.Views
{
    using System;
    using BoardGames.Desktop.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

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
            this.ViewModel.OnLoginSuccess = (roleName) =>
            {
                this.Frame?.Navigate(typeof(MenuBarPage));
            };

            this.ViewModel.OnNavigateToRegister = () =>
            {
                this.Frame?.Navigate(typeof(RegisterPage));
            };
        }

        private async void ForgotPassword_Click(object pointerSender, RoutedEventArgs eventArgs)
        {
            await this.ResetPasswordDialog.ShowAsync();
        }
    }
}