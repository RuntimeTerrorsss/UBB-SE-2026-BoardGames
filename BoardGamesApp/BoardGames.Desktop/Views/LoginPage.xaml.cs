namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;

    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<LoginViewModel>();
            this.DataContext = this.ViewModel;

            this.InitializeNavigationCallbacks();
        }

        public LoginViewModel ViewModel { get; }

        private void InitializeNavigationCallbacks()
        {
            this.ViewModel.OnLoginSuccess = (roleName) =>
            {
                App.OnUserLoggedIn();
            };

            this.ViewModel.OnNavigateToRegister = () =>
            {
                App.NavigateTo(typeof(RegisterPage));
            };
        }

        private async void ForgotPassword_Click(object pointerSender, RoutedEventArgs eventArgs)
        {
            await this.ResetPasswordDialog.ShowAsync();
        }
    }
}
