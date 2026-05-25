namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using BoardGames.Desktop.Services;
    using BoardGames.Data.Constants;
    using BoardGames.Shared.DTO;
    using BoardGames.Desktop.Services;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Media.Imaging;
    using ApiAccountService = BoardGames.Shared.ProxyServices.IAccountService;
    using ApiAuthService = BoardGames.Shared.ProxyServices.IAuthService;

    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly ApiAccountService accountService;
        private readonly ApiAuthService authService;
        private readonly IFilePickerService filePickerService;
        private readonly ISessionContext sessionContext;

        private string pendingAvatarPath = string.Empty;
        private string username = string.Empty;
        private string displayName = string.Empty;
        private string email = string.Empty;
        private string phoneNumber = string.Empty;
        private string country = string.Empty;
        private string city = string.Empty;
        private string streetName = string.Empty;
        private string streetNumber = string.Empty;
        private string avatarUrl = string.Empty;
        private string currentPassword = string.Empty;
        private string newPassword = string.Empty;
        private string confirmPassword = string.Empty;

        private string emailError = string.Empty;
        private string displayNameError = string.Empty;
        private string phoneError = string.Empty;
        private string streetNumberError = string.Empty;
        private string confirmPasswordError = string.Empty;
        private string currentPasswordError = string.Empty;
        private string newPasswordError = string.Empty;

        public ProfileViewModel(
            ApiAccountService accountService,
            ApiAuthService authService,
            IFilePickerService filePickerService,
            ISessionContext sessionContext)
        {
            this.accountService = accountService;
            this.authService = authService;
            this.filePickerService = filePickerService;
            this.sessionContext = sessionContext;

            AvailableCountries.Clear();

            foreach (var currentCountry in DomainConstants.CountryList)
            {
                AvailableCountries.Add(currentCountry);
            }

            SaveProfileCommand = new AsyncRelayCommand(this.SaveProfileAsync);
            RemoveAvatarCommand = new AsyncRelayCommand(this.RemoveAvatarAsync);
            SelectAvatarCommand = new AsyncRelayCommand(this.SelectAvatarAsync);
            SaveNewPasswordCommand = new AsyncRelayCommand(this.SaveNewPasswordAsync);
            SignOutCommand = new AsyncRelayCommand(this.SignOutAsync);
        }

        public Action OnSignOutSuccess { get; set; }

        public ObservableCollection<string> AvailableCountries { get; } = new();

        public string Username { get => username; set => this.SetProperty(ref username, value); }
        public string DisplayName { get => displayName; set => this.SetProperty(ref displayName, value); }
        public string Email { get => email; set => this.SetProperty(ref email, value); }
        public string PhoneNumber { get => phoneNumber; set => this.SetProperty(ref phoneNumber, value); }
        public string Country { get => country; set => this.SetProperty(ref country, value); }
        public string City { get => city; set => this.SetProperty(ref city, value); }
        public string StreetName { get => streetName; set => this.SetProperty(ref streetName, value); }
        public string StreetNumber { get => streetNumber; set => this.SetProperty(ref streetNumber, value); }
        public string CurrentPassword { get => currentPassword; set => this.SetProperty(ref currentPassword, value); }
        public string NewPassword { get => newPassword; set => this.SetProperty(ref newPassword, value); }
        public string ConfirmPassword { get => confirmPassword; set => this.SetProperty(ref confirmPassword, value); }

        public string ConfirmPasswordError { get => confirmPasswordError; set => this.SetProperty(ref confirmPasswordError, value); }
        public string CurrentPasswordError { get => currentPasswordError; set => this.SetProperty(ref currentPasswordError, value); }
        public string NewPasswordError { get => newPasswordError; set => this.SetProperty(ref newPasswordError, value); }
        public string EmailError { get => emailError; set => this.SetProperty(ref emailError, value); }
        public string DisplayNameError { get => displayNameError; set => this.SetProperty(ref displayNameError, value); }
        public string PhoneError { get => phoneError; set => this.SetProperty(ref phoneError, value); }
        public string StreetNumberError { get => streetNumberError; set => this.SetProperty(ref streetNumberError, value); }

        public IEnumerable<string> Countries => DomainConstants.CountryList;

        public ICommand SaveProfileCommand { get; }
        public ICommand SelectAvatarCommand { get; }
        public ICommand RemoveAvatarCommand { get; }
        public ICommand SaveNewPasswordCommand { get; }
        public ICommand SignOutCommand { get; }

        public string AvatarUrl
        {
            get => avatarUrl;
            set
            {
                if (this.SetProperty(ref avatarUrl, value))
                {
                    this.OnPropertyChanged(nameof(ProfileImage));
                }
            }
        }

        public ImageSource ProfileImage
        {
            get
            {
                if (string.IsNullOrEmpty(AvatarUrl))
                {
                    return null;
                }

                try
                {
                    return new BitmapImage(new Uri(AvatarUrl));
                }
                catch (UriFormatException)
                {
                    return null;
                }
            }
        }

        public async Task LoadProfileAsync()
        {
            this.IsLoading = true;

            Username = sessionContext.Username;
            DisplayName = sessionContext.DisplayName;
            Email = sessionContext.Email;
            PhoneNumber = sessionContext.PhoneNumber;
            Country = sessionContext.Country;
            City = sessionContext.City;
            StreetName = sessionContext.StreetName;
            StreetNumber = sessionContext.StreetNumber;

            this.OnPropertyChanged(nameof(ProfileImage));

            Guid currentAccountId = sessionContext.AccountId;
            var profileResult = await accountService.GetProfileAsync(currentAccountId);

            if (profileResult.Success && profileResult.Data != null)
            {
                this.ApplyProfile(profileResult.Data);
            }

            this.IsLoading = false;
        }

        private async Task SaveProfileAsync()
        {
            this.IsLoading = true;
            ClearErrors();

            Guid currentAccountId = sessionContext.AccountId;

            AccountProfileDataTransferObject updateInformation = new AccountProfileDataTransferObject
            {
                DisplayName = DisplayName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                Country = Country,
                City = City,
                StreetName = StreetName,
                StreetNumber = StreetNumber
            };

            var updateResult = await accountService.UpdateProfileAsync(currentAccountId, updateInformation);

            if (updateResult.Success)
            {
                if (!string.IsNullOrEmpty(pendingAvatarPath))
                {
                    var avatarUploadResult = await accountService.UploadAvatarAsync(currentAccountId, pendingAvatarPath);
                    if (!avatarUploadResult.Success)
                    {
                        this.ProcessValidationErrors(avatarUploadResult.Error);
                        this.IsLoading = false;
                        return;
                    }

                    AvatarUrl = avatarUploadResult.Data ?? string.Empty;
                    pendingAvatarPath = string.Empty;
                }

                var refreshedProfileResult = await accountService.GetProfileAsync(currentAccountId);
                if (refreshedProfileResult.Success && refreshedProfileResult.Data != null)
                {
                    sessionContext.Populate(refreshedProfileResult.Data);
                    this.ApplyProfile(refreshedProfileResult.Data);
                }

                this.ErrorMessage = "Profile saved successfully.";
            }
            else
            {
                this.ProcessValidationErrors(updateResult.Error);
            }

            this.IsLoading = false;
        }

        private async Task SelectAvatarAsync()
        {
            try
            {
                string selectedFilePath = await filePickerService.PickImageFileAsync();

                if (selectedFilePath != null)
                {
                    pendingAvatarPath = selectedFilePath;
                    AvatarUrl = selectedFilePath;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                this.ErrorMessage = "Access to the file was denied: " + ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private async Task RemoveAvatarAsync()
        {
            var removeAvatarResult = await accountService.RemoveAvatarAsync(sessionContext.AccountId);
            if (removeAvatarResult.Success)
            {
                AvatarUrl = string.Empty;
                pendingAvatarPath = string.Empty;
                return;
            }

            this.ErrorMessage = removeAvatarResult.Error ?? "Failed to remove avatar.";
        }

        private async Task SaveNewPasswordAsync()
        {
            ConfirmPasswordError = string.Empty;
            CurrentPasswordError = string.Empty;
            NewPasswordError = string.Empty;
            this.ErrorMessage = string.Empty;

            if (NewPassword != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords do not match.";
                return;
            }

            var passwordChangeResult = await accountService.ChangePasswordAsync(
                sessionContext.AccountId,
                CurrentPassword,
                NewPassword);

            if (passwordChangeResult.Success)
            {
                sessionContext.Clear();
                this.ErrorMessage = "Password updated. Redirecting to login...";
                await Task.Delay(2000);
                await SignOutAsync();
            }
            else
            {
                this.ErrorMessage = passwordChangeResult.Error;
            }
        }

        private async Task SignOutAsync()
        {
            await authService.LogoutAsync();
            sessionContext.Clear();
            OnSignOutSuccess?.Invoke();
        }

        private void ApplyProfile(AccountProfileDataTransferObject profile)
        {
            Username = profile.Username;
            DisplayName = profile.DisplayName;
            Email = profile.Email;
            PhoneNumber = profile.PhoneNumber;
            Country = profile.Country;
            City = profile.City;
            StreetName = profile.StreetName;
            StreetNumber = profile.StreetNumber;
            AvatarUrl = profile.AvatarUrl;
        }

        private void ClearErrors()
        {
            EmailError = string.Empty;
            DisplayNameError = string.Empty;
            PhoneError = string.Empty;
            StreetNumberError = string.Empty;
            this.ErrorMessage = string.Empty;
        }

        private void ProcessValidationErrors(string errorString)
        {
            if (string.IsNullOrWhiteSpace(errorString))
            {
                return;
            }

            string[] errors = errorString.Split(';');
            foreach (string error in errors)
            {
                string[] parts = error.Split('|');
                if (parts.Length < 2)
                {
                    continue;
                }

                switch (parts[0])
                {
                    case "Email":
                        EmailError = parts[1];
                        break;
                    case "DisplayName":
                        DisplayNameError = parts[1];
                        break;
                    case "PhoneNumber":
                        PhoneError = parts[1];
                        break;
                    case "StreetNumber":
                        StreetNumberError = parts[1];
                        break;
                    default:
                        this.ErrorMessage = parts[1];
                        break;
                }
            }
        }
    }
}
