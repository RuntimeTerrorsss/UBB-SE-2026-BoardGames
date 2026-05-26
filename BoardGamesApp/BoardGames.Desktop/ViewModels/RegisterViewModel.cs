using BoardGames.Desktop.Helpers;

namespace BoardGames.Desktop.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;
        private readonly SessionService sessionService;

        private string username = string.Empty;
        private string displayName = string.Empty;
        private string email = string.Empty;
        private string password = string.Empty;
        private string confirmPassword = string.Empty;
        private string city = string.Empty;
        private string country = string.Empty;
        private bool isLoading;

        private string usernameError = string.Empty;
        private string displayNameError = string.Empty;
        private string emailError = string.Empty;
        private string passwordError = string.Empty;
        private string confirmPasswordError = string.Empty;
        private string cityError = string.Empty;
        private string countryError = string.Empty;
        private string errorMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action? NavigateToLogin;

        public event Action? NavigateToHome;

        public ICommand RegisterCommand { get; }

        public ICommand GoToLoginCommand { get; }
        public ICommand GoToHomeCommand { get; }

        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        public string DisplayName
        {
            get => displayName;
            set { displayName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set { confirmPassword = value; OnPropertyChanged(); }
        }

        public string City
        {
            get => city;
            set { city = value; OnPropertyChanged(); }
        }

        public string Country
        {
            get => country;
            set { country = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommandNoParam)?.RaiseCanExecuteChanged();
            }
        }

        public string UsernameError
        {
            get => usernameError;
            set { usernameError = value; OnPropertyChanged(); }
        }

        public string DisplayNameError
        {
            get => displayNameError;
            set { displayNameError = value; OnPropertyChanged(); }
        }

        public string EmailError
        {
            get => emailError;
            set { emailError = value; OnPropertyChanged(); }
        }

        public string PasswordError
        {
            get => passwordError;
            set { passwordError = value; OnPropertyChanged(); }
        }

        public string ConfirmPasswordError
        {
            get => confirmPasswordError;
            set { confirmPasswordError = value; OnPropertyChanged(); }
        }

        public string CityError
        {
            get => cityError;
            set { cityError = value; OnPropertyChanged(); }
        }

        public string CountryError
        {
            get => countryError;
            set { countryError = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set { errorMessage = value; OnPropertyChanged(); }
        }

        public RegisterViewModel(IUserService userService, SessionService sessionService)
        {
            this.userService = userService;
            this.sessionService = sessionService;
            RegisterCommand = new RelayCommandNoParam(RegisterAsync, () => !IsLoading);
            GoToLoginCommand = new RelayCommandNoParam(() => NavigateToLogin?.Invoke());
            GoToHomeCommand = new RelayCommandNoParam(() => NavigateToHome?.Invoke());
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task RegisterAsync()
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
                var username = Username.Trim();
                var password = Password;

                var newUser = new User
                {
                    Username = username,
                    DisplayName = DisplayName.Trim(),
                    Email = Email.Trim(),
                    PasswordHash = password,
                    City = City.Trim(),
                    Country = Country.Trim(),
                };

                var result = await userService.RegisterUserAsync(newUser);

                if (!result)
                {
                    RunOnUiThread(() =>
                        ErrorMessage = "Registration failed. The username or email may already be taken.");
                    return;
                }

                var loggedInUser = await userService.LoginAsync(username, password);
                if (loggedInUser == null)
                {
                    RunOnUiThread(() =>
                        ErrorMessage = "Account created, but automatic sign-in failed. Please sign in manually.");
                    return;
                }

                RunOnUiThread(() =>
                {
                    AuthSession.SetAuthenticatedUser(loggedInUser, sessionService);
                    NavigateToHome?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration failed: {ex}");
                RunOnUiThread(() =>
                    ErrorMessage = "Registration failed. Please check your connection and try again.");
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
            UsernameError = Username.Trim().Length < 3 ? "Username must be at least 3 characters." : string.Empty;
            DisplayNameError = string.IsNullOrWhiteSpace(DisplayName) ? "Display name is required." : string.Empty;
            EmailError = string.IsNullOrWhiteSpace(Email) || !Email.Contains('@')
                ? "Invalid email address."
                : string.Empty;
            PasswordError = Password.Length < 6 ? "Password must be at least 6 characters." : string.Empty;
            ConfirmPasswordError = Password != ConfirmPassword ? "Passwords do not match." : string.Empty;
            CityError = string.IsNullOrWhiteSpace(City) ? "City is required." : string.Empty;
            CountryError = string.IsNullOrWhiteSpace(Country) ? "Country is required." : string.Empty;

            System.Diagnostics.Debug.WriteLine("validation");
            System.Diagnostics.Debug.WriteLine($"{UsernameError}");
            System.Diagnostics.Debug.WriteLine($"{DisplayNameError}");
            System.Diagnostics.Debug.WriteLine($"{EmailError}");
            System.Diagnostics.Debug.WriteLine($"{PasswordError}");
            System.Diagnostics.Debug.WriteLine($"{ConfirmPasswordError}");
            System.Diagnostics.Debug.WriteLine($"{CityError}");
            System.Diagnostics.Debug.WriteLine($"{CountryError}");

            return string.IsNullOrEmpty(UsernameError)
                && string.IsNullOrEmpty(DisplayNameError)
                && string.IsNullOrEmpty(EmailError)
                && string.IsNullOrEmpty(PasswordError)
                && string.IsNullOrEmpty(ConfirmPasswordError)
                && string.IsNullOrEmpty(CityError)
                && string.IsNullOrEmpty(CountryError);
        }
    }
}
