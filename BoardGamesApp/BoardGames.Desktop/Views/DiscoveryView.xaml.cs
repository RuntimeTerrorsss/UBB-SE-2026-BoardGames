
using BoardGames.Desktop.Helpers;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Views.ChatViews;

namespace BoardGames.Desktop.Views
{
    public sealed partial class DiscoveryView : Page
    {
        public DiscoveryView()
        {
            this.InitializeComponent();
        }
        public FilteredSearchViewModel ViewModel { get; private set; } = null!;
        protected async override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);

            this.UpdateAuthUi();

            var criteria = navigationArgs.Parameter as FilterCriteria ?? new FilterCriteria();
            this.ViewModel = new FilteredSearchViewModel(App.SearchAndFilterService, App.GlobalGeographicalService);

            this.ViewModel.OnGameSelectedRequest += gameId =>
            {
                if (!this.EnsureLoggedIn())
                {
                    return;
                }

                this.Frame.Navigate(typeof(GameDetailsView), gameId);
            };

            this.StartDatePicker.Date = this.ViewModel.SelectedStartDate;
            this.EndDatePicker.Date = this.ViewModel.SelectedEndDate;

            await this.ViewModel.Initialize(criteria);
            this.DataContext = this.ViewModel;
        }

        private void UpdateAuthUi()
        {
            bool isLoggedIn = AuthSession.IsLoggedIn;
            this.LoginButton.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            this.DashboardButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            this.ChatButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            this.LogoutButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Game_Click(object sender, ItemClickEventArgs itemClickedArgs)
        {
            if (!this.EnsureLoggedIn())
            {
                return;
            }

            if (itemClickedArgs.ClickedItem is GameDTO game)
            {
                this.Frame.Navigate(typeof(GameDetailsView), game.GameId);
            }
        }

        private bool EnsureLoggedIn()
        {
            if (AuthSession.IsLoggedIn)
            {
                return true;
            }

            _ = this.ShowLoginRequiredDialogAsync();
            return false;
        }

        private async System.Threading.Tasks.Task ShowLoginRequiredDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Login required",
                Content = "You need to log in to book a game.",
                PrimaryButtonText = "Login",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(LoginView));
            }
        }

        private void EndDatePicker_DayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            if (this.ViewModel?.SelectedStartDate.HasValue == true)
            {
                var date = args.Item.Date.Date;
                var selectedStartDate = this.ViewModel.SelectedStartDate.Value.Date;

                if (date < selectedStartDate)
                {
                    args.Item.IsBlackout = true;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(ChatPageView), SessionContext.GetInstance().UserId);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(PaymentHistoryView), SessionContext.GetInstance().UserId);
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(NotificationsPage));
        }

        private void GamesButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(ListingsPage));
        }

        private void AccountButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (!AuthSession.IsLoggedIn)
            {
                _ = this.ShowLoginRequiredDialogAsync();
                return;
            }

            this.Frame.Navigate(typeof(ProfilePage));
        }
    }
}