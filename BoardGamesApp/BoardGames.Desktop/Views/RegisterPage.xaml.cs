namespace BoardRentAndProperty.Views
{
    using BoardGames.Desktop.ViewModels;

    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<RegisterViewModel>();
            this.DataContext = this.ViewModel;

            this.InitializeNavigationCallbacks();
        }

        public RegisterViewModel ViewModel { get; }

        private void InitializeNavigationCallbacks()
        {
            this.ViewModel.OnRegistrationSuccess = () =>
            {
                App.OnUserLoggedIn();
            };

            this.ViewModel.OnNavigateBackRequest = () =>
            {
                App.NavigateBack();
            };
        }
    }
}
