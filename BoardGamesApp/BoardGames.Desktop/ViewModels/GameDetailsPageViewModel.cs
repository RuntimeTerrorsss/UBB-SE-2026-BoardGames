// <copyright file="GameDetailsPageViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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
        private readonly IGameService gameService;
        private readonly IRequestService requestService;
        private readonly IRentalService rentalService;
        private readonly ISessionContext sessionContext;
        private readonly List<BookedDateRangeDTO> bookedRanges = new();
        private readonly List<BookedDateRangeDTO> pendingRequestRanges = new();
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

        public GameDetailsPageViewModel(
            IGameService gameService,
            IRequestService requestService,
            IRentalService rentalService,
            ISessionContext sessionContext)
        {
            this.gameService = gameService;
            this.requestService = requestService;
            this.rentalService = rentalService;
            this.sessionContext = sessionContext;
            this.apiBaseUri = ResolveApiBaseUri();
        }

        public IReadOnlyList<BookedDateRangeDTO> BookedRanges => this.bookedRanges;

        public IReadOnlyList<BookedDateRangeDTO> PendingRequestRanges => this.pendingRequestRanges;

        public Action? OnCalendarRangesLoaded { get; set; }

        public Action? OnBackRequested { get; set; }

        public Action<string>? OnLoginRequested { get; set; }

        public Action<GameDetailDTO, DateTime, DateTime>? OnNavigateToConfirm { get; set; }

        public string PriceText => Game is null ? string.Empty : $"{Game.Price:0.##} RON / day";

        public string PlayerRangeText => Game is null ? string.Empty : $"{Game.MinimumPlayerNumber} - {Game.MaximumPlayerNumber} players";

        public string TotalPriceText
        {
            get
            {
                if (Game is null || !TryGetSelectedDates(out var start, out var end))
                {
                    return "0 RON";
                }

                int days = (end - start).Days + 1;
                return $"{days * Game.Price:0.##} RON";
            }
        }

        public Visibility LoginButtonVisibility => sessionContext.IsLoggedIn ? Visibility.Collapsed : Visibility.Visible;

        public bool CanSubmit => !IsLoading;

        public async Task LoadAsync(int gameId)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
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
                await LoadCalendarRangesAsync(gameId);
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
        private void SubmitRequest()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

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
                ErrorMessage = "Please choose a date on the calendar.";
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

            OnNavigateToConfirm?.Invoke(Game, start, end);
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

        public bool IsDateUnavailable(DateTime date)
        {
            var day = date.Date;
            if (day < DateTime.Today)
            {
                return true;
            }

            foreach (var range in this.bookedRanges)
            {
                if (day >= range.StartDate.Date && day <= range.EndDate.Date)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task LoadCalendarRangesAsync(int gameId)
        {
            this.bookedRanges.Clear();
            this.pendingRequestRanges.Clear();

            var bookedDatesResult = await this.rentalService.GetBookedDatesForGameAsync(gameId);
            if (!bookedDatesResult.Success)
            {
                ErrorMessage = bookedDatesResult.Error ?? "Could not load booked dates.";
                return;
            }

            foreach (var bookedRange in bookedDatesResult.Data ?? Array.Empty<BookedDateRangeDTO>())
            {
                this.bookedRanges.Add(bookedRange);
            }

            if (sessionContext.IsLoggedIn)
            {
                var requestsResult = await this.requestService.GetRequestsForRenterAsync(sessionContext.AccountId);
                if (requestsResult.Success)
                {
                    foreach (var request in requestsResult.Data ?? Array.Empty<RequestDTO>())
                    {
                        if (request.Game?.Id == gameId && request.Status == RequestStatus.Open)
                        {
                            this.pendingRequestRanges.Add(new BookedDateRangeDTO
                            {
                                StartDate = request.StartDate,
                                EndDate = request.EndDate,
                            });
                        }
                    }
                }
            }

            OnCalendarRangesLoaded?.Invoke();
        }

        private bool TryGetSelectedDates(out DateTime start, out DateTime end)
        {
            if (!StartDate.HasValue)
            {
                start = default;
                end = default;
                return false;
            }

            start = StartDate.Value.Date;
            end = EndDate?.Date ?? start;
            return true;
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
