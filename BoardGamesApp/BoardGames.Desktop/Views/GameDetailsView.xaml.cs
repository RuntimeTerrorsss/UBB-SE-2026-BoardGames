
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class GameDetailsView : Page
    {
        private DateTime? selectedDateStart;
        private DateTime? selectedDateEnd;
        public GameDetailsView()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            if (eventArgs.Parameter is not int gameId)
            {
                return;
            }

            var viewModel = new GameDetailsViewModel(App.BookingService, gameId);

            this.DataContext = viewModel;

            viewModel.OnGoBackRequested += () =>
            {
                if (this.Frame.CanGoBack) this.Frame.GoBack();
            };

            viewModel.OnStartBookingRequested += (bookingDto, range) =>
            {
                this.Frame.Navigate(typeof(ConfirmBookingView), (bookingDto, range));
            };

            viewModel.OnMessageRequested += async message =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Booking",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
            };

            viewModel.OnChatWithOwnerRequested += (currentUserId, ownerUserId) =>
            {
                this.Frame.Navigate(typeof(ChatViews.ChatPageView), (currentUserId, ownerUserId));
            };

            await viewModel.InitializeAsync();
            this.ForceRedrawCalendar();
        }

        private void OnBackClicked(object sender, RoutedEventArgs eventArgs)
        {
            var viewModel = (GameDetailsViewModel)this.DataContext;
            viewModel.GoBack();
        }

        private async void OnBookClicked(object sender, RoutedEventArgs eventArgs)
        {
            var selectedDates = this.RentalCalendar.SelectedDates;
            if (selectedDates.Count == 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Invalid selection",
                    Content = "Please select at least one date.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
                return;
            }

            var viewModel = (GameDetailsViewModel)this.DataContext;
            var sortedDates = selectedDates
               .Select(date => date.DateTime)
               .OrderBy(date => date)
               .ToList();
            var dateRange = new TimeRange(sortedDates[0], sortedDates[selectedDates.Count - 1]);
            viewModel.StartBooking(dateRange);
        }

        private void OnDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs eventArgs)
        {
            if (this.DataContext is not GameDetailsViewModel viewModel)
            {
                return;
            }

            var selectedDates = this.RentalCalendar.SelectedDates;

            if (selectedDates.Count > 2)
            {
                var datesToKeep = new List<DateTimeOffset>
                    {
                        selectedDates[selectedDates.Count - 2],
                        selectedDates[selectedDates.Count - 1],
                    };
                this.RentalCalendar.SelectedDates.Clear();
                foreach (var date in datesToKeep)
                {
                    this.RentalCalendar.SelectedDates.Add(date);
                }

                return;
            }

            if (selectedDates.Count < 1)
            {
                this.selectedDateStart = null;
                this.selectedDateEnd = null;
                this.ForceRedrawCalendar();
                return;
            }

            var sorted = selectedDates
                .Select(date => date.DateTime)
                .OrderBy(date => date)
                .ToList();

            this.selectedDateStart = sorted[0];
            this.selectedDateEnd = sorted[sorted.Count - 1];

            var range = new TimeRange(this.selectedDateStart.Value, this.selectedDateEnd.Value);
            viewModel.CalculatePrice(range);

            this.ForceRedrawCalendar();
        }

        private void ForceRedrawCalendar()
        {
            var currentDate = this.RentalCalendar.MinDate;
            this.RentalCalendar.MinDate = currentDate.AddDays(1);
            this.RentalCalendar.MinDate = currentDate;
        }

        private void OnDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs eventArgs)
        {
            if (this.DataContext is not GameDetailsViewModel viewModel)
            {
                return;
            }

            var date = eventArgs.Item.Date.Date;
            var today = DateTimeOffset.Now.Date;

            if (date < today)
            {
                eventArgs.Item.IsBlackout = true;
                return;
            }

            bool isUnavailable = false;
            if (viewModel.UnavailableTimeRanges != null)
            {
                foreach (var range in viewModel.UnavailableTimeRanges)
                {
                    if (date >= range.StartTime.Date && date <= range.EndTime.Date)
                    {
                        isUnavailable = true;
                        break;
                    }
                }
            }

            if (isUnavailable)
            {
                eventArgs.Item.IsBlackout = true;
                eventArgs.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                return;
            }

            if (this.selectedDateStart.HasValue && this.selectedDateEnd.HasValue &&
                date >= this.selectedDateStart.Value.Date && date <= this.selectedDateEnd.Value.Date)
            {
                eventArgs.Item.IsBlackout = false;
                eventArgs.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod);
                return;
            }

            eventArgs.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        }
    }
}
