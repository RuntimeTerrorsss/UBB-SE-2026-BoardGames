using BoardGames.Desktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BoardGames.Desktop.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterView : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterView()
        {
            InitializeComponent();
            ViewModel = new RegisterViewModel(App.UserService, App.Session);
            ViewModel.NavigateToLogin += () => Frame.Navigate(typeof(LoginView));
            ViewModel.NavigateToHome += () => Frame.Navigate(typeof(DiscoveryView));
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.IsLoading))
                {
                    RegisterButton.IsEnabled = !ViewModel.IsLoading;
                    RegisterButton.Content = ViewModel.IsLoading ? "Creating account…" : "Create account";
                }

                if (e.PropertyName == nameof(ViewModel.ErrorMessage))
                {
                    ErrorBar.IsOpen = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
                    ErrorBar.Message = ViewModel.ErrorMessage;
                }
            };

            ViewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.UsernameError):
                        UsernameErrorText.Text = ViewModel.UsernameError;
                        UsernameErrorText.Visibility = string.IsNullOrEmpty(ViewModel.UsernameError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case nameof(ViewModel.DisplayNameError):
                        DisplayNameErrorText.Text = ViewModel.DisplayNameError;
                        DisplayNameErrorText.Visibility = string.IsNullOrEmpty(ViewModel.DisplayNameError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case nameof(ViewModel.EmailError):
                        EmailErrorText.Text = ViewModel.EmailError;
                        EmailErrorText.Visibility = string.IsNullOrEmpty(ViewModel.EmailError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case nameof(ViewModel.PasswordError):
                        PasswordErrorText.Text = ViewModel.PasswordError;
                        PasswordErrorText.Visibility = string.IsNullOrEmpty(ViewModel.PasswordError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case nameof(ViewModel.ConfirmPasswordError):
                        ConfirmPasswordErrorText.Text = ViewModel.ConfirmPasswordError;
                        ConfirmPasswordErrorText.Visibility = string.IsNullOrEmpty(ViewModel.ConfirmPasswordError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case nameof(ViewModel.CityError):
                        CityErrorText.Text = ViewModel.CityError;
                        CityErrorText.Visibility = string.IsNullOrEmpty(ViewModel.CityError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case nameof(ViewModel.CountryError):
                        CountryErrorText.Text = ViewModel.CountryError;
                        CountryErrorText.Visibility = string.IsNullOrEmpty(ViewModel.CountryError) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                }
            };
            DataContext = ViewModel;

            // PasswordBox can't use Binding
            PasswordInput.PasswordChanged += (s, e) => ViewModel.Password = PasswordInput.Password;
            ConfirmPasswordInput.PasswordChanged += (s, e) => ViewModel.ConfirmPassword = ConfirmPasswordInput.Password;
        }
    }
}
