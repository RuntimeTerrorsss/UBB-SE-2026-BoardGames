// <copyright file="RentalDatePicker.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace BoardGames.Desktop.Views.Controls
{
    public sealed partial class RentalDatePicker : UserControl
    {
        private static readonly string[] WeekdayLabels = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
        private static readonly SolidColorBrush UnavailableBrush = new(ColorHelper.FromArgb(255, 220, 53, 69));
        private static readonly SolidColorBrush PendingRequestBrush = new(ColorHelper.FromArgb(255, 255, 193, 7));
        private static readonly SolidColorBrush PendingRequestForeground = new(ColorHelper.FromArgb(255, 26, 26, 46));
        private static readonly SolidColorBrush SelectedBrush = new(ColorHelper.FromArgb(255, 67, 97, 238));
        private static readonly SolidColorBrush InRangeBrush = new(ColorHelper.FromArgb(255, 199, 210, 254));
        private static readonly SolidColorBrush DayDefaultBrush = new(ColorHelper.FromArgb(255, 35, 48, 74));
        private static readonly SolidColorBrush PastBrush = new(ColorHelper.FromArgb(255, 42, 53, 72));
        private static readonly SolidColorBrush HoverBrush = new(ColorHelper.FromArgb(255, 47, 63, 92));
        private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);
        private static readonly SolidColorBrush DayForeground = new(ColorHelper.FromArgb(255, 232, 236, 244));
        private static readonly SolidColorBrush PastForeground = new(ColorHelper.FromArgb(255, 173, 181, 189));
        private static readonly SolidColorBrush InRangeForeground = new(ColorHelper.FromArgb(255, 26, 26, 46));
        private static readonly SolidColorBrush WhiteForeground = new(Colors.White);

        private readonly List<BookedDateRangeDTO> bookedRanges = new();
        private readonly List<BookedDateRangeDTO> pendingRequestRanges = new();
        private readonly List<Button> dayButtons = new();
        private DateTime viewMonth;
        private DateTime? startDate;
        private DateTime? endDate;

        public RentalDatePicker()
        {
            InitializeComponent();
            viewMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ConfigureCalendarGrid(WeekdayRow, 1);
            ConfigureCalendarGrid(DayGrid, 6);
            BuildWeekdayLabels();
            BuildDayButtons();
            RenderMonth();
        }

        private static void ConfigureCalendarGrid(Grid grid, int rows)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            for (int column = 0; column < 7; column++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int row = 0; row < rows; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
        }

        public event EventHandler? SelectionChanged;

        public DateTime? StartDate => startDate;

        public DateTime? EndDate => endDate;

        public void SetBookedRanges(IEnumerable<BookedDateRangeDTO>? ranges)
        {
            bookedRanges.Clear();
            if (ranges is not null)
            {
                bookedRanges.AddRange(ranges);
            }

            RenderMonth();
        }

        public void SetPendingRequestRanges(IEnumerable<BookedDateRangeDTO>? ranges)
        {
            pendingRequestRanges.Clear();
            if (ranges is not null)
            {
                pendingRequestRanges.AddRange(ranges);
            }

            RenderMonth();
        }

        public void ClearSelection()
        {
            startDate = null;
            endDate = null;
            UpdateSelectionLabel();
            RenderMonth();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void BuildWeekdayLabels()
        {
            WeekdayRow.Children.Clear();
            for (int column = 0; column < WeekdayLabels.Length; column++)
            {
                var label = new TextBlock
                {
                    Text = WeekdayLabels[column],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 12,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 201, 210, 227)),
                    Margin = new Thickness(0, 0, 0, 4),
                };

                Grid.SetColumn(label, column);
                Grid.SetRow(label, 0);
                WeekdayRow.Children.Add(label);
            }
        }

        private void BuildDayButtons()
        {
            DayGrid.Children.Clear();
            dayButtons.Clear();

            for (int i = 0; i < 42; i++)
            {
                var button = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(6),
                    FontSize = 13,
                    Padding = new Thickness(0),
                    Margin = new Thickness(2),
                    MinHeight = 36,
                    MinWidth = 36,
                    Visibility = Visibility.Collapsed,
                };

                Grid.SetColumn(button, i % 7);
                Grid.SetRow(button, i / 7);
                button.Click += OnDayButtonClick;
                dayButtons.Add(button);
                DayGrid.Children.Add(button);
            }
        }

        private void RenderMonth()
        {
            MonthTitle.Text = viewMonth.ToString("MMMM yyyy");

            int year = viewMonth.Year;
            int month = viewMonth.Month;
            var firstDay = new DateTime(year, month, 1);
            int startOffset = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            var today = DateTime.Today;

            for (int i = 0; i < dayButtons.Count; i++)
            {
                var button = dayButtons[i];
                if (i < startOffset || i >= startOffset + daysInMonth)
                {
                    button.Visibility = Visibility.Collapsed;
                    button.IsEnabled = false;
                    button.Tag = null;
                    continue;
                }

                int day = i - startOffset + 1;
                var date = new DateTime(year, month, day);
                button.Visibility = Visibility.Visible;
                button.Content = day.ToString();
                button.Tag = date;

                ApplyDayStyle(button, date, today);
            }
        }

        private void ApplyDayStyle(Button button, DateTime date, DateTime today)
        {
            if (date.Date < today)
            {
                button.IsEnabled = false;
                button.Background = PastBrush;
                button.Foreground = PastForeground;
                return;
            }

            if (IsDateBooked(date))
            {
                button.IsEnabled = false;
                button.Background = UnavailableBrush;
                button.Foreground = WhiteForeground;
                return;
            }

            if (IsDateInPendingRequest(date))
            {
                button.IsEnabled = false;
                button.Background = PendingRequestBrush;
                button.Foreground = PendingRequestForeground;
                return;
            }

            button.IsEnabled = true;

            if (startDate.HasValue && endDate.HasValue
                && date >= startDate.Value.Date
                && date <= endDate.Value.Date)
            {
                bool isEndpoint = date == startDate.Value.Date || date == endDate.Value.Date;
                if (isEndpoint)
                {
                    button.Background = SelectedBrush;
                    button.Foreground = WhiteForeground;
                }
                else
                {
                    button.Background = InRangeBrush;
                    button.Foreground = InRangeForeground;
                }

                return;
            }

            button.Background = DayDefaultBrush;
            button.Foreground = DayForeground;
        }

        private bool IsDateBooked(DateTime date)
        {
            var day = date.Date;
            foreach (var range in bookedRanges)
            {
                if (day >= range.StartDate.Date && day <= range.EndDate.Date)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDateInPendingRequest(DateTime date)
        {
            var day = date.Date;
            foreach (var range in pendingRequestRanges)
            {
                if (day >= range.StartDate.Date && day <= range.EndDate.Date)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDayButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: DateTime date })
            {
                return;
            }

            SelectDate(date);
        }

        private void SelectDate(DateTime date)
        {
            bool hasRange = startDate.HasValue
                && endDate.HasValue
                && startDate.Value.Date != endDate.Value.Date;

            if (!startDate.HasValue || hasRange)
            {
                startDate = date.Date;
                endDate = date.Date;
            }
            else if (date.Date < startDate.Value.Date)
            {
                endDate = startDate.Value.Date;
                startDate = date.Date;
            }
            else
            {
                endDate = date.Date;
            }

            UpdateSelectionLabel();
            RenderMonth();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateSelectionLabel()
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value.Date == endDate.Value.Date)
                {
                    SelectionLabel.Text = $"Selected: {startDate.Value:dd MMM yyyy}";
                }
                else
                {
                    SelectionLabel.Text = $"Selected: {startDate.Value:dd MMM yyyy} to {endDate.Value:dd MMM yyyy}";
                }

                return;
            }

            SelectionLabel.Text = "Choose a date on the calendar";
        }

        private void OnPrevMonthClick(object sender, RoutedEventArgs e)
        {
            viewMonth = viewMonth.AddMonths(-1);
            RenderMonth();
        }

        private void OnNextMonthClick(object sender, RoutedEventArgs e)
        {
            viewMonth = viewMonth.AddMonths(1);
            RenderMonth();
        }
    }
}
