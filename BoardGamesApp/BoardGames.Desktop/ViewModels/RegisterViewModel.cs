using System.Collections.ObjectModel;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoardGames.Desktop.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService authService;

        [ObservableProperty]
        private string displayName = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        [ObservableProperty]
        private string country = string.Empty;

        [ObservableProperty]
        private string city = string.Empty;

        [ObservableProperty]
        private string streetName = string.Empty;

        [ObservableProperty]
        private string streetNumber = string.Empty;

        [ObservableProperty]
        private string displayNameError = string.Empty;

        [ObservableProperty]
        private string usernameError = string.Empty;

        [ObservableProperty]
        private string emailError = string.Empty;

        [ObservableProperty]
        private string passwordError = string.Empty;

        [ObservableProperty]
        private string confirmPasswordError = string.Empty;

        [ObservableProperty]
        private string phoneNumberError = string.Empty;

        [ObservableProperty]
        private string successMessage = string.Empty;

        public RegisterViewModel(IAuthService authService)
        {
            this.authService = authService;
            AvailableCountries = new ObservableCollection<string>
            {
                string.Empty,
                "Romania",
                "Hungary",
                "Germany",
                "France",
                "Italy",
            };
        }

        public ObservableCollection<string> AvailableCountries { get; }

        public Action<string>? OnRegistrationSuccess { get; set; }

        public Action? OnNavigateToLogin { get; set; }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;

            if (!Validate())
            {
                return;
            }

            IsLoading = true;

            try
            {
                var request = new RegisterDTO
                {
                    DisplayName = DisplayName.Trim(),
                    Username = Username.Trim(),
                    Email = Email.Trim(),
                    Password = Password,
                    ConfirmPassword = ConfirmPassword,
                    PhoneNumber = PhoneNumber.Trim(),
                    Country = Country.Trim(),
                    City = City.Trim(),
                    StreetName = StreetName.Trim(),
                    StreetNumber = StreetNumber.Trim(),
                };

                var result = await authService.RegisterAsync(request);
                if (!result.Success)
                {
                    ErrorMessage = result.Error ?? "Registration failed.";
                    return;
                }

                SuccessMessage = "Account created successfully.";
                OnRegistrationSuccess?.Invoke("Account created successfully. Please sign in.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoToLogin()
        {
            OnNavigateToLogin?.Invoke();
        }

        private bool Validate()
        {
            DisplayNameError = string.IsNullOrWhiteSpace(DisplayName) ? "Display name is required." : string.Empty;
            UsernameError = string.IsNullOrWhiteSpace(Username) ? "Username is required." : string.Empty;
            EmailError = string.IsNullOrWhiteSpace(Email) || !Email.Contains('@') ? "A valid email is required." : string.Empty;
            PasswordError = Password.Length < 6 ? "Password must be at least 6 characters." : string.Empty;
            ConfirmPasswordError = Password != ConfirmPassword ? "Passwords do not match." : string.Empty;
            PhoneNumberError = string.Empty;

            return string.IsNullOrEmpty(DisplayNameError)
                && string.IsNullOrEmpty(UsernameError)
                && string.IsNullOrEmpty(EmailError)
                && string.IsNullOrEmpty(PasswordError)
                && string.IsNullOrEmpty(ConfirmPasswordError)
                && string.IsNullOrEmpty(PhoneNumberError);
        }
    }
}
