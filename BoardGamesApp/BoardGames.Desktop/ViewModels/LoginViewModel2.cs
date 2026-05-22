namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using BoardRentAndProperty.ApiClient;
    using BoardRentAndProperty.Contracts.DataTransferObjects;
    using BoardRentAndProperty.Utilities;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
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

        public LoginViewModel(IAuthService authService, ISessionContext sessionContext)
        {
            this.authService = authService;
            this.sessionContext = sessionContext;
        }

        public LoginViewModel(IAuthService authService)
            : this(authService, new SessionContext())
        {
        }

        public Action<string> OnLoginSuccess { get; set; }

        public Action OnNavigateToRegister { get; set; }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(this.UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username/email and password.";
                return;
            }

            IsLoading = true;

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = this.UsernameOrEmail,
                Password = Password,
                RememberMe = this.RememberMe,
            };

            try
            {
                var loginResult = await authService.LoginAsync(loginRequest);

                if (loginResult.Success && loginResult.Data != null)
                {
                    sessionContext.Populate(loginResult.Data);
                    string userRole = loginResult.Data.Role?.Name ?? AppRoles.StandardUser;
                    OnLoginSuccess?.Invoke(userRole);
                }
                else
                {
                    ErrorMessage = loginResult.Error ?? "Login failed.";
                }
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
