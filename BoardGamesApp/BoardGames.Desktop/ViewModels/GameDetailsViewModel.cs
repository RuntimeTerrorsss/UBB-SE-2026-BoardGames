using BoardGames.Desktop.Commands;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.Threading.Tasks;

namespace BoardGames.Desktop.ViewModels
{
    public class GameDetailsViewModel : INotifyPropertyChanged
    {
        private const decimal DefaultTotalPrice = 0;
        private readonly IGameService _gameService;
        private readonly IRequestService _requestService;

        private readonly int _gameId;

        private bool _hasError;
        private decimal _totalPrice;
        private GameSummaryDTO _gameDetails;
        private BookedDateRangeDTO[] _unavailableTimeRanges = Array.Empty<BookedDateRangeDTO>();

        public event Action<int, Guid>? OnChatWithOwnerRequested;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? OnGoBackRequested;
        public event Action<GameSummaryDTO, DateTime, DateTime>? OnStartBookingRequested;
        public event Action<string>? OnMessageRequested;

        public GameDetailsViewModel(IGameService gameService, IRequestService requestService, int gameId)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _gameId = gameId;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var result = await _gameService.GetGameByIdAsync(_gameId);

                if (result.IsSuccess && result.Data != null)
                {
                    GameDetails = result.Data;
                    var datesResult = await _requestService.GetBookedDatesAsync(_gameId);
                    if (datesResult.IsSuccess && datesResult.Data != null)
                    {
                        UnavailableTimeRanges = datesResult.Data.ToArray();
                    }

                    HasError = false;
                }
                else
                {
                    throw new Exception(result.ErrorMessage ?? "Game not found in API.");
                }
            }
            catch (Exception exception)
            {
                HasError = true;
                UnavailableTimeRanges = Array.Empty<BookedDateRangeDTO>();
                OnMessageRequested?.Invoke($"Could not load game details. {exception.Message}");
            }
        }

        public DateTime Today => DateTime.Now.Date;

        public GameSummaryDTO GameDetails
        {
            get => _gameDetails;
            private set
            {
                _gameDetails = value;
                OnPropertyChanged();
            }
        }

        public bool HasError
        {
            get => _hasError;
            private set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            private set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        public BookedDateRangeDTO[] UnavailableTimeRanges
        {
            get => _unavailableTimeRanges;
            private set
            {
                _unavailableTimeRanges = value;
                OnPropertyChanged();
            }
        }

        public ICommand GoBackCommand => new RelayCommand(_ => GoBack());

        public ICommand ChatWithOwnerCommand => new RelayCommand(_ =>
        {
            if (GameDetails != null)
            {
                OnChatWithOwnerRequested?.Invoke(0, GameDetails.OwnerAccountId);
            }
        });
        public async Task<bool> CheckGameAvailability(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = await _requestService.CheckAvailabilityAsync(_gameId, startDate, endDate);
                return result.IsSuccess && result.Data;
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not check API availability. {exception.Message}");
                return false;
            }
        }

        public decimal CalculatePrice(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate) throw new ArgumentException("Start date cannot be after end date.");
                int days = (endDate - startDate).Days + 1;
                TotalPrice = days * GameDetails.Price;
                return TotalPrice;
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not calculate price. {exception.Message}");
                TotalPrice = DefaultTotalPrice;
                return DefaultTotalPrice;
            }
        }

        public void StartBooking(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate == default || endDate == default || startDate > endDate)
                {
                    OnMessageRequested?.Invoke("Please select a valid booking date range.");
                    return;
                }
                OnStartBookingRequested?.Invoke(GameDetails, startDate, endDate);
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not continue to booking. {exception.Message}");
            }
        }

        public void GoBack()
        {
            try
            {
                OnGoBackRequested?.Invoke();
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not go back. {exception.Message}");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}