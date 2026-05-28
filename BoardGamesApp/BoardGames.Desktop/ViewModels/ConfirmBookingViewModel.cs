using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Desktop.Services; 

namespace BoardGames.Desktop.ViewModels
{
    public class ConfirmBookingViewModel : INotifyPropertyChanged
    {
        private readonly IRequestService _requestService;
        private readonly ISessionContext _sessionContext;

        private GameSummaryDTO _gameDetails;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _totalPrice;

        public event Action? OnGoBackRequested;
        public event Action? OnConfirmBookingRequested;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string>? OnErrorOccurred;

        public ConfirmBookingViewModel(IRequestService requestService, ISessionContext sessionContext)
        {
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
        }

        public void Initialize(GameSummaryDTO gameDetails, DateTime startDate, DateTime endDate)
        {
            GameDetails = gameDetails ?? throw new ArgumentNullException(nameof(gameDetails));
            StartDate = startDate;
            EndDate = endDate;
            TotalPrice = CalculatePrice();
        }

        public GameSummaryDTO GameDetails
        {
            get => _gameDetails;
            private set { _gameDetails = value; OnPropertyChanged(); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            private set
            {
                _startDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedStartDate));
                OnPropertyChanged(nameof(NumberOfDays));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            private set
            {
                _endDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedEndDate));
                OnPropertyChanged(nameof(NumberOfDays));
            }
        }

        public string FormattedStartDate => StartDate.ToString("dd MMM yyyy");
        public string FormattedEndDate => EndDate.ToString("dd MMM yyyy");

        public int NumberOfDays => (EndDate - StartDate).Days + 1;

        public decimal TotalPrice
        {
            get => _totalPrice;
            private set { _totalPrice = value; OnPropertyChanged(); }
        }

        public decimal CalculatePrice()
        {
            if (GameDetails == null) return 0;
            return NumberOfDays * GameDetails.Price;
        }

        public async Task ConfirmBookingAsync()
        {
            try
            {

                if (_sessionContext.AccountId == Guid.Empty)
                {
                    RaiseError("User not logged in. Please log in first.");
                    return;
                }

                if (_sessionContext.AccountId == GameDetails.OwnerAccountId)
                {
                    RaiseError("You cannot rent a game you already own.");
                    return;
                }

                var requestDto = new CreateRequestDTO
                {
                    GameId = GameDetails.GameId,
                    RenterAccountId = _sessionContext.AccountId,
                    OwnerAccountId = GameDetails.OwnerAccountId,
                    StartDate = this.StartDate,
                    EndDate = this.EndDate
                };

                var result = await _requestService.CreateRequestAsync(requestDto);

                if (result.IsSuccess)
                {
                    OnConfirmBookingRequested?.Invoke();
                }
                else
                {
                    RaiseError(result.ErrorMessage ?? "Failed to submit rental request.");
                }
            }
            catch (Exception exception)
            {
                RaiseError($"Could not confirm booking. {exception.Message}");
            }
        }

        public void GoBack() => OnGoBackRequested?.Invoke();

        private void RaiseError(string message) => OnErrorOccurred?.Invoke(message);

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}