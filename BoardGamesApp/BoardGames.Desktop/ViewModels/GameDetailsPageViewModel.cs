// <copyright file="GameDetailsPageViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Configuration;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BoardGames.Desktop.ViewModels
{
    public partial class GameDetailsPageViewModel : BaseViewModel
    {
        private const string EmptyTotalPriceText = "0 RON";
        private const int InclusiveDateRangeDayOffset = 1;

        private readonly IGameService gameService;
        private readonly IRequestService requestService;
        private readonly ISessionContext sessionContext;
        private readonly Uri apiBaseUri;

        [ObservableProperty]
        private GameDetailDTO? game;

        [ObservableProperty]
        private BitmapImage? gameImage;

        [ObservableProperty]
        private DateTimeOffset? startDate;

        [ObservableProperty]
        private DateTimeOffset? endDate;

        [ObservableProperty]
        private string successMessage = string.Empty;

        [ObservableProperty]
        private string availabilityMessage = string.Empty;

        public GameDetailsPageViewModel(
            IGameService gameService,
            IRequestService requestService,
            ISessionContext sessionContext)
        {
            this.gameService = gameService;
            this.requestService = requestService;
            this.sessionContext = sessionContext;
            this.apiBaseUri = ResolveApiBaseUri();
        }

        public ObservableCollection<string> BookedDateRanges { get; } = new();

        public Action? OnBackRequested { get; set; }

        public Action<string>? OnLoginRequested { get; set; }

        public Action? OnRequestSuccess { get; set; }

        public string PriceText => Game is null ? string.Empty : $"{Game.Price:0.##} RON / day";

        public string PlayerRangeText => Game is null ? string.Empty : $"{Game.MinimumPlayerNumber} - {Game.MaximumPlayerNumber} players";

        public string TotalPriceText
        {
            get
            {
                if (Game is null || !TryGetSelectedDates(out var start, out var end))
                {
                    return EmptyTotalPriceText;
                }

                int days = (end - start).Days + InclusiveDateRangeDayOffset;
                return $"{days * Game.Price:0.##} RON";
            }
        }

        public Visibility LoginButtonVisibility => sessionContext.IsLoggedIn ? Visibility.Collapsed : Visibility.Visible;

        public bool CanSubmit => !IsLoading;

        public async Task LoadAsync(int gameId)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            AvailabilityMessage = string.Empty;
            IsLoading = true;

            try
            {
                var gameResult = await gameService.GetGameDetailsByIdAsync(gameId);
                if (!gameResult.Success || gameResult.Data is null)
                {
                    ErrorMessage = gameResult.Error ?? "Game not found.";
                    return;
                }

                Game = gameResult.Data;
                GameImage = CreateImage(Game.ImageUrl, apiBaseUri);
                NotifyGameDisplayChanged();
                await LoadBookedDatesAsync(gameId);
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        [RelayCommand]
        private void Back()
        {
            OnBackRequested?.Invoke();
        }

        [RelayCommand]
        private void Login()
        {
            OnLoginRequested?.Invoke("Please sign in before sending a rental request.");
        }

        [RelayCommand]
        private async Task SubmitRequestAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            AvailabilityMessage = string.Empty;

            if (Game is null)
            {
                ErrorMessage = "Game details are not loaded yet.";
                return;
            }

            if (!sessionContext.IsLoggedIn)
            {
                OnLoginRequested?.Invoke("Please sign in before sending a rental request.");
                return;
            }

            if (sessionContext.AccountId == Game.OwnerAccountId)
            {
                ErrorMessage = "You cannot rent a game you already own.";
                return;
            }

            if (!TryGetSelectedDates(out var start, out var end))
            {
                ErrorMessage = "Please choose a start date and an end date.";
                return;
            }

            if (start < DateTime.Today)
            {
                ErrorMessage = "Please choose today or a future date.";
                return;
            }

            if (end < start)
            {
                ErrorMessage = "End date cannot be earlier than the start date.";
                return;
            }

            IsLoading = true;
            OnPropertyChanged(nameof(CanSubmit));

            try
            {
                var availability = await requestService.CheckAvailabilityAsync(Game.Id, start, end);
                if (!availability.Success)
                {
                    ErrorMessage = availability.Error ?? "Could not check availability.";
                    return;
                }

                if (!availability.Data)
                {
                    ErrorMessage = "The selected dates are unavailable.";
                    return;
                }

                var createResult = await requestService.CreateRequestAsync(new CreateRequestDTO
                {
                    GameId = Game.Id,
                    RenterAccountId = sessionContext.AccountId,
                    OwnerAccountId = Game.OwnerAccountId,
                    StartDate = start,
                    EndDate = end,
                });

                if (!createResult.Success)
                {
                    ErrorMessage = DescribeCreateFailure(createResult);
                    return;
                }

                SuccessMessage = "Rental request sent. Redirecting to chat...";
                await LoadBookedDatesAsync(Game.Id);
                OnRequestSuccess?.Invoke();
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        partial void OnGameChanged(GameDetailDTO? value)
        {
            NotifyGameDisplayChanged();
        }

        partial void OnStartDateChanged(DateTimeOffset? value)
        {
            OnPropertyChanged(nameof(TotalPriceText));
        }

        partial void OnEndDateChanged(DateTimeOffset? value)
        {
            OnPropertyChanged(nameof(TotalPriceText));
        }

        private async Task LoadBookedDatesAsync(int gameId)
        {
            BookedDateRanges.Clear();

            var now = DateTime.Today;
            var bookedDatesResult = await requestService.GetBookedDatesAsync(gameId, now.Month, now.Year);
            if (!bookedDatesResult.Success)
            {
                AvailabilityMessage = bookedDatesResult.Error ?? "Could not load booked dates.";
                return;
            }

            foreach (var bookedRange in bookedDatesResult.Data ?? Array.Empty<BookedDateRangeDTO>())
            {
                BookedDateRanges.Add($"{bookedRange.StartDate:dd MMM yyyy} - {bookedRange.EndDate:dd MMM yyyy}");
            }

            AvailabilityMessage = BookedDateRanges.Count == 0
                ? "No booked dates are known for this month."
                : "Booked dates this month:";
        }

        private bool TryGetSelectedDates(out DateTime start, out DateTime end)
        {
            start = StartDate?.Date ?? default;
            end = EndDate?.Date ?? default;
            return StartDate.HasValue && EndDate.HasValue;
        }

        private void NotifyGameDisplayChanged()
        {
            OnPropertyChanged(nameof(PriceText));
            OnPropertyChanged(nameof(PlayerRangeText));
            OnPropertyChanged(nameof(TotalPriceText));
        }

        private static string DescribeCreateFailure(ServiceResult failure)
        {
            return RequestErrorMapper.MapCreate(failure) switch
            {
                CreateRequestError.OwnerCannotRent => "You cannot rent a game you already own.",
                CreateRequestError.DatesUnavailable => "The selected dates are unavailable.",
                CreateRequestError.GameDoesNotExist => "This game could not be found.",
                CreateRequestError.InvalidDateRange => "The selected date range is invalid.",
                _ => failure.Error ?? "Could not send the rental request.",
            };
        }

        private static BitmapImage? CreateImage(string imageUrl, Uri apiBaseUri)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            try
            {
                var imageUri = Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri)
                    ? absoluteUri
                    : new Uri(apiBaseUri, imageUrl.TrimStart('/'));

                return new BitmapImage(imageUri);
            }
            catch
            {
                return null;
            }
        }

        private static Uri ResolveApiBaseUri()
        {
            string? configuredBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.Trim();

            if (string.IsNullOrWhiteSpace(configuredBaseUrl) || !Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException("ApiBaseUrl is not configured correctly in App.config.");
            }

            return baseUri;
        }
    }
}
