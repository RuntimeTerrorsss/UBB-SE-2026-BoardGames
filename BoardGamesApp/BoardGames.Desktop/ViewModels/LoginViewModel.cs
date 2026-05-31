using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BookingBoardGames.Sharing.Services;

namespace BoardGames.Desktop.ViewModels
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
            LoginCommand = new RelayCommandNoParam(async () => await LoginAsync(), () => !IsLoading);
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

            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await userService.LoginAsync(Identifier.Trim(), Password);

            if (user == null)
            {
                ErrorMessage = "Invalid username/email or password.";
                IsLoading = false;
                return;
            }

            sessionService.SetUser(user.Id, user.Username, user.DisplayName);
            IsLoading = false;
            NavigateToHome?.Invoke();
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
