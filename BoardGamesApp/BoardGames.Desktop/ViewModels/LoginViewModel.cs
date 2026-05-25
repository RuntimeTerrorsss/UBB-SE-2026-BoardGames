using BoardGames.Desktop.ViewModels;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Src.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BookingBoardGames.Src.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;
        private readonly SessionService sessionService;

        private string identifier = string.Empty;
        private string password = string.Empty;
        private string errorMessage = string.Empty;
        private string identifierError = string.Empty;
        private string passwordError = string.Empty;
        private bool isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action? NavigateToHome;

        public event Action? NavigateToRegister;

        public ICommand LoginCommand { get; }

        public ICommand GoToRegisterCommand { get; }

        public string Identifier
        {
            get => identifier;
            set { identifier = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set { errorMessage = value; OnPropertyChanged(); }
        }

        public string IdentifierError
        {
            get => identifierError;
            set { identifierError = value; OnPropertyChanged(); }
        }

        public string PasswordError
        {
            get => passwordError;
            set { passwordError = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
                (LoginCommand as RelayCommandNoParam)?.RaiseCanExecuteChanged();
            }
        }

        public LoginViewModel(IUserService userService, SessionService sessionService)
        {
            this.userService = userService;
            this.sessionService = sessionService;
            LoginCommand = new RelayCommandNoParam(LoginAsync, () => !IsLoading);
            GoToRegisterCommand = new RelayCommandNoParam(() => NavigateToRegister?.Invoke());
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task LoginAsync()
        {
            if (!ValidateUser())
            {
                return;
            }

            RunOnUiThread(() =>
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
            });

            try
            {
                var user = await userService.LoginAsync(Identifier.Trim(), Password);

                if (user == null)
                {
                    RunOnUiThread(() => ErrorMessage = "Invalid username/email or password.");
                    return;
                }

                RunOnUiThread(() =>
                {
                    AuthSession.SetAuthenticatedUser(user, sessionService);
                    NavigateToHome?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login failed: {ex}");
                RunOnUiThread(() =>
                    ErrorMessage = "Sign-in failed. Please check your connection and try again.");
            }
            finally
            {
                RunOnUiThread(() => IsLoading = false);
            }
        }

        private static void RunOnUiThread(Action action)
        {
            var dispatcher = ((App)Microsoft.UI.Xaml.Application.Current).Window?.DispatcherQueue;
            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => action());
            }
            else
            {
                action();
            }
        }

        private bool ValidateUser()
        {
            IdentifierError = string.IsNullOrWhiteSpace(Identifier) ? "Username or email is required." : string.Empty;
            PasswordError = string.IsNullOrWhiteSpace(Password) ? "Password is required." : string.Empty;

            return string.IsNullOrEmpty(IdentifierError)
                && string.IsNullOrEmpty(PasswordError);
        }
    }
}
