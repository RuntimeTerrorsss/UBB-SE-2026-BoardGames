// <copyright file="ConfirmBookingView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;

namespace BookingBoardGames.Src.Views;
/// <summary>
/// Provides the user interface for confirming a booking, allowing date modification and final submission.
/// </summary>
public sealed partial class ConfirmBookingView : Page
{
    private const int MinimumSelectedDates = 1;
    private DateTime? modifySelectedStart;
    private DateTime? modifySelectedEnd;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmBookingView"/> class.
    /// </summary>
    public ConfirmBookingView()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
    /// </summary>
    /// <param name="eventArgs">Event data that can be examined by overriding code.</param>
    protected override async void OnNavigatedTo(NavigationEventArgs eventArgs)
    {
        base.OnNavigatedTo(eventArgs);

        if (eventArgs.Parameter is not (BookingDTO bookingDTO, TimeRange range))
        {
            return;
        }

        var viewModel = new ConfirmBookingViewModel(App.BookingService, bookingDTO, range);
        await viewModel.InitializeAsync(bookingDTO);

        viewModel.OnErrorOccurred += async (message) =>
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await dialog.ShowAsync();
        };

        viewModel.OnGoBackRequested += () =>
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        };

        viewModel.OnConfirmBookingRequested += async () =>
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = "Booking request was sent successfully!",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await dialog.ShowAsync();
            this.Frame.Navigate(typeof(DiscoveryView));
        };

        this.DataContext = viewModel;
    }

    private void OnBackClicked(object sender, RoutedEventArgs eventArgs)
    {
        var viewModel = (ConfirmBookingViewModel)this.DataContext;
        viewModel.GoBack();
    }

    private async void OnModifyClicked(object sender, RoutedEventArgs eventArgs)
    {
        var viewModel = (ConfirmBookingViewModel)this.DataContext;

        var calendar = new CalendarView
        {
            SelectionMode = CalendarViewSelectionMode.Multiple,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinDate = DateTimeOffset.Now.Date,
            SelectedBorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
            SelectedHoverBorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
            SelectedPressedBorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
        };

        calendar.CalendarViewDayItemChanging += (calendarSender, calendarArguments) =>
        {
            var date = calendarArguments.Item.Date.DateTime;

            if (viewModel.IsTimeRangeUnavailable(date))
            {
                calendarArguments.Item.IsBlackout = true;
                calendarArguments.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                return;
            }

            calendarArguments.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        };

        calendar.SelectedDatesChanged += (calendarSender, calendarArguments) =>
        {
            var selectedDates = calendarSender.SelectedDates;
            if (selectedDates.Count > MinimumSelectedDates + 1)
            {
                var toKeep = new List<DateTimeOffset>
            {
                selectedDates[selectedDates.Count - 2],
                selectedDates[selectedDates.Count - 1],
            };
                calendarSender.SelectedDates.Clear();
                foreach (var date in toKeep)
                    calendarSender.SelectedDates.Add(date);
                return;
            }

            if (selectedDates.Count < MinimumSelectedDates)
            {
                this.modifySelectedStart = null;
                this.modifySelectedEnd = null;
                return;
            }

            var sorted = selectedDates.Select(date => date.DateTime).OrderBy(date => date).ToList();
            this.modifySelectedStart = sorted[0];
            this.modifySelectedEnd = sorted[sorted.Count - 1];
        };

        calendar.Loaded += async (sender, eventArgs) =>
        {
            await Task.Delay(200);
            calendar.DispatcherQueue.TryEnqueue(() =>
            {
                calendar.MinDate = DateTimeOffset.Now.Date.AddDays(1);
                calendar.MinDate = DateTimeOffset.Now.Date;
            });
        };

        calendar.SelectedDates.Add(viewModel.SelectedTimeRange.StartTime);
        if (viewModel.SelectedTimeRange.EndTime != viewModel.SelectedTimeRange.StartTime)
            calendar.SelectedDates.Add(viewModel.SelectedTimeRange.EndTime);

        var dialog = new ContentDialog
        {
            Title = "Modify dates",
            Content = calendar,
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot,
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var selectedDates = calendar.SelectedDates;
            if (selectedDates.Count < MinimumSelectedDates) return;

            var sorted = selectedDates.Select(date => date.DateTime).OrderBy(date => date).ToList();
            viewModel.UpdateSelectedRange(new TimeRange(sorted[0], sorted[sorted.Count - 1]));
        }
    }

    private async void OnConfirmClicked(object sender, RoutedEventArgs eventArgs)
    {
        var viewModel = (ConfirmBookingViewModel)this.DataContext;
        await viewModel.ConfirmBooking();
    }

    private void OnMessageUserClicked(object sender, RoutedEventArgs eventArgs)
    {
        var viewModel = (ConfirmBookingViewModel)this.DataContext;
        int currentUserId = BookingBoardGames.Data.Enum.SessionContext.GetInstance().UserId;
        this.Frame.Navigate(typeof(ChatViews.ChatPageView), (currentUserId, viewModel.GameAndUserDetails.UserId));
    }
}
