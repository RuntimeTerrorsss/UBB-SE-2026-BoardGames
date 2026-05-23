namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using BoardGames.Desktop.Services;
    using BoardGames.Shared;
    using BoardGames.Shared.DTO;
    using CommunityToolkit.Mvvm.Input;

    public class AdminViewModel : PagedViewModel<AccountProfileDTO>
    {
        private const string AdminAccessDeniedMessage = "Unauthorized access. Administrator role is required.";

        private readonly IAdminService adminService;
        private readonly IDesktopAuthorizationService authorizationService;
        private AccountProfileDTO selectedAccount;
        private string errorMessage;
        private bool isLoading;

        public AdminViewModel(IAdminService adminService, IDesktopAuthorizationService authorizationService)
        {
            this.adminService = adminService;
            this.authorizationService = authorizationService;

            SuspendAccountCommand = new AsyncRelayCommand(this.SuspendAccountAsync, this.CanModifySelectedAccount);
            UnsuspendAccountCommand = new AsyncRelayCommand(this.UnsuspendAccountAsync, this.CanModifySelectedAccount);
            UnlockAccountCommand = new AsyncRelayCommand(this.UnlockAccountAsync, this.CanModifySelectedAccount);
            NextPageCommand = new RelayCommand(this.ExecuteNextPage);
            PreviousPageCommand = new RelayCommand(this.ExecutePreviousPage);
        }

        public AdminViewModel(IAdminService adminService)
            : this(adminService, new AlwaysAuthorizedDesktopAuthorizationService())
        {
        }

        public IAsyncRelayCommand SuspendAccountCommand { get; }
        public IAsyncRelayCommand UnsuspendAccountCommand { get; }
        public IAsyncRelayCommand UnlockAccountCommand { get; }
        public IRelayCommand NextPageCommand { get; }
        public IRelayCommand PreviousPageCommand { get; }

        public AccountProfileDataTransferObject SelectedAccount
        {
            get => selectedAccount;
            set
            {
                if (selectedAccount != value)
                {
                    selectedAccount = value;
                    OnPropertyChanged(nameof(SelectedAccount));
                    SuspendAccountCommand.NotifyCanExecuteChanged();
                    UnsuspendAccountCommand.NotifyCanExecuteChanged();
                    UnlockAccountCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        protected override void Reload()
        {
            _ = LoadAccountsAsync();
        }

        public async Task LoadAccountsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (!authorizationService.IsAdministrator)
            {
                ErrorMessage = AdminAccessDeniedMessage;
                IsLoading = false;
                return;
            }

            var serviceResult = await adminService.GetAllAccountsAsync(CurrentPage, PageSize);

            if (serviceResult.Success && serviceResult.Data != null)
            {
                this.SetAllItems(serviceResult.Data.ToImmutableList());
            }
            else
            {
                ErrorMessage = serviceResult.Error ?? "Failed to load accounts.";
            }

            IsLoading = false;
        }

        public async Task ResetPasswordWithValueAsync(string newPassword)
        {
            if (SelectedAccount == null)
            {
                return;
            }

            if (!authorizationService.IsAdministrator)
            {
                ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

            var serviceResult = await adminService.ResetPasswordAsync(SelectedAccount.Id, newPassword);
            ErrorMessage = serviceResult.Success ? "Password reset successful." : serviceResult.Error;
        }

        private async Task SuspendAccountAsync()
        {
            if (!authorizationService.IsAdministrator)
            {
                ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

            var result = await adminService.SuspendAccountAsync(SelectedAccount.Id);
            if (result.Success)
            {
                await LoadAccountsAsync();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        private async Task UnsuspendAccountAsync()
        {
            if (!authorizationService.IsAdministrator)
            {
                ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

            var result = await adminService.UnsuspendAccountAsync(SelectedAccount.Id);
            if (result.Success)
            {
                await LoadAccountsAsync();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        private async Task UnlockAccountAsync()
        {
            if (!authorizationService.IsAdministrator)
            {
                ErrorMessage = AdminAccessDeniedMessage;
                return;
            }

            var result = await adminService.UnlockAccountAsync(SelectedAccount.Id);
            ErrorMessage = result.Success ? "Account unlocked." : result.Error;
        }

        private void ExecuteNextPage() => NextPage();

        private void ExecutePreviousPage() => PrevPage();

        private bool CanModifySelectedAccount() => SelectedAccount != null;

        private sealed class AlwaysAuthorizedDesktopAuthorizationService : IDesktopAuthorizationService
        {
            public Guid CurrentAccountId => Guid.Empty;

            public bool IsLoggedIn => true;

            public bool IsAdministrator => true;

            public bool CanAccessPage(Type pageType) => true;

            public bool CanAccessMenuPage(AppPage page) => true;
        }
    }
}
