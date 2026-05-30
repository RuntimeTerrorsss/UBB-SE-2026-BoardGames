using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class RegisterView : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterView()
        {
            InitializeComponent();
            ViewModel = new RegisterViewModel(App.UserService, App.Session);
            ViewModel.NavigateToLogin += () => Frame.Navigate(typeof(LoginView));
            ViewModel.NavigateToHome += () => Frame.Navigate(typeof(DiscoveryView));
            ViewModel.PropertyChanged += (viewModelSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(ViewModel.IsLoading))
                {
                    RegisterButton.IsEnabled = !ViewModel.IsLoading;
                    RegisterButton.Content = ViewModel.IsLoading ? "Creating accountÃ¢â‚¬Â¦" : "Create account";
                }

                if (propertyChangedEventArgs.PropertyName == nameof(ViewModel.ErrorMessage))
                {
                    ErrorBar.IsOpen = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
                    ErrorBar.Message = ViewModel.ErrorMessage;
                }
            };

            ViewModel.PropertyChanged += (viewModelSender, propertyChangedEventArgs) =>
            {
                switch (propertyChangedEventArgs.PropertyName)
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
            PasswordInput.PasswordChanged += (passwordInputSender, passwordChangedEventArgs) => ViewModel.Password = PasswordInput.Password;
            ConfirmPasswordInput.PasswordChanged += (confirmPasswordInputSender, confirmPasswordChangedEventArgs) => ViewModel.ConfirmPassword = ConfirmPasswordInput.Password;
        }
    }
}
