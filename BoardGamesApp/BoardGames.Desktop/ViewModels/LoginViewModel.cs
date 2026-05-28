using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoardGames.Desktop.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService authService;
        private readonly ISessionContext sessionContext;

        [ObservableProperty]
        private string usernameOrEmail = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        [ObservableProperty]
        private string infoMessage = string.Empty;

        public LoginViewModel(IAuthService authService, ISessionContext sessionContext)
        {
            this.authService = authService;
            this.sessionContext = sessionContext;
        }

        public Action? OnLoginSuccess { get; set; }

        public Action? OnNavigateToRegister { get; set; }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username/email and password.";
                return;
            }

            IsLoading = true;

            try
            {
                var loginRequest = new LoginDTO
                {
                    UsernameOrEmail = UsernameOrEmail.Trim(),
                    Password = Password,
                    RememberMe = RememberMe,
                };

                var loginResult = await authService.LoginAsync(loginRequest);
                if (loginResult.Success && loginResult.Data is not null)
                {
                    sessionContext.Populate(loginResult.Data);
                    ErrorMessage = string.Empty;
                    InfoMessage = string.Empty;
                    OnLoginSuccess?.Invoke();
                    return;
                }

                ErrorMessage = loginResult.Error ?? "Login failed.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            OnNavigateToRegister?.Invoke();
        }
    }
}
