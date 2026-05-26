using BoardGames.Desktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BoardGames.Desktop.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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

            // PasswordBox can't use Binding
            PasswordInput.PasswordChanged += (s, e) => ViewModel.Password = PasswordInput.Password;

            ViewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
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
                        LoginButton.Content = ViewModel.IsLoading ? "Signing in…" : "Sign in";
                        break;
                }
            };
        }
    }
}
