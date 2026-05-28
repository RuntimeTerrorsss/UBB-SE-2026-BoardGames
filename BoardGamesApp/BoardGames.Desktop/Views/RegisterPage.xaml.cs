namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.Extensions.DependencyInjection;

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
            this.ViewModel.OnRegistrationSuccess = () =>
            {
                this.Frame?.Navigate(typeof(MenuBarPage));
            };

            this.ViewModel.OnNavigateBackRequest = () =>
            {
                if (this.Frame != null && this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            };
        }
    }
}