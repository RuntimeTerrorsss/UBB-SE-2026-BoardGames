using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class LoginView : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginView()
        {
            InitializeComponent();
            ViewModel = new LoginViewModel(App.UserService, App.Session);
            ViewModel.NavigateToHome += () => Frame.Navigate(typeof(DiscoveryView));
            ViewModel.NavigateToRegister += () => Frame.Navigate(typeof(RegisterView));

            DataContext = ViewModel;
            PasswordInput.PasswordChanged += (passwordInputSender, passwordChangedEventArgs) => ViewModel.Password = PasswordInput.Password;

            ViewModel.PropertyChanged += (viewModelSender, propertyChangedEventArgs) =>
            {
                switch (propertyChangedEventArgs.PropertyName)
                {
                    case nameof(ViewModel.IdentifierError):
                        ErrorText.Text = ViewModel.IdentifierError;
                        ErrorText.Visibility = string.IsNullOrEmpty(ViewModel.IdentifierError)
                            ? Visibility.Collapsed : Visibility.Visible;
                        break;

                    case nameof(ViewModel.PasswordError):
                        PasswordErrorText.Text = ViewModel.PasswordError;
                        PasswordErrorText.Visibility = string.IsNullOrEmpty(ViewModel.PasswordError)
                            ? Visibility.Collapsed : Visibility.Visible;
                        break;

                    case nameof(ViewModel.ErrorMessage):
                        ErrorBar.IsOpen = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
                        ErrorBar.Message = ViewModel.ErrorMessage;
                        break;

                    case nameof(ViewModel.IsLoading):
                        LoginButton.IsEnabled = !ViewModel.IsLoading;
                        LoginButton.Content = ViewModel.IsLoading ? "Signing inâ€¦" : "Sign in";
                        break;
                }
            };
        }
    }
}
