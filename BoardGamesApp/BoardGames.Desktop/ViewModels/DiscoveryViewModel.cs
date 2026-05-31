using BoardGames.Desktop.Commands;
using BoardGames.Desktop.Helpers;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;

namespace BoardGames.Desktop.ViewModels
{
    public class DiscoveryViewModel : INotifyPropertyChanged
    {
        private const int ItemsPerPage = 10;
        private const int FirstPageNumber = 1;
        private const int DefaultMinimumPlayers = 1;
        private const int DefaultMaximumPrice = 100;

        private readonly IGameService _gameService;
        private ObservableCollection<GameSummaryDTO> _allFetchedGames = new();
        private ObservableCollection<GameSummaryDTO> _visibleGames = new();

        private int _currentPage = FirstPageNumber;
        private int _totalAvailableGamesCount;
        private string _citySearchText = string.Empty;
        private string _searchName;
        private int _selectedMinimumPlayers = DefaultMinimumPlayers;
        private int _selectedMaximumPrice = DefaultMaximumPrice;

        public DiscoveryViewModel(IGameService gameService)
        {
            _gameService = gameService;

            NextPageCommand = new RelayCommand(_ => GoToNextPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage());
            SearchCommand = new RelayCommand(_ => ExecuteSearch());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            LoadInitialGamesAsync();
        }

        public event Action<int>? OnGameSelectedRequest;
        public event Action<string>? OnErrorOccurred;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<GameSummaryDTO> VisibleGames
        {
            get => _visibleGames;
            set { _visibleGames = value; OnPropertyChanged(nameof(VisibleGames)); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
        }

        public int TotalPages => _totalAvailableGamesCount == 0 ? 0 : (int)Math.Ceiling((double)_totalAvailableGamesCount / ItemsPerPage);

        public string CitySearchText
        {
            get => _citySearchText;
            set { _citySearchText = value; OnPropertyChanged(nameof(CitySearchText)); }
        }

        public string SearchName
        {
            get => _searchName;
            set { _searchName = value; OnPropertyChanged(nameof(SearchName)); }
        }

        public int SelectedMinimumPlayers
        {
            get => _selectedMinimumPlayers;
            set { _selectedMinimumPlayers = value; OnPropertyChanged(nameof(SelectedMinimumPlayers)); }
        }

        public int SelectedMaximumPrice
        {
            get => _selectedMaximumPrice;
            set { _selectedMaximumPrice = value; OnPropertyChanged(nameof(SelectedMaximumPrice)); }
        }

        public string NoResultsMessage => _totalAvailableGamesCount == 0 ? "No games available." : string.Empty;

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private async void LoadInitialGamesAsync()
        {
            try
            {
                var result = await _gameService.GetAllGamesAsync();

                if (result.IsSuccess)
                {
                    _allFetchedGames.Clear();
                    foreach (var game in result.Data)
                    {
                        _allFetchedGames.Add(game);
                    }

                    ExecuteSearch();
                }
                else
                {
                    OnErrorOccurred?.Invoke(result.ErrorMessage ?? "Failed to load games from API.");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Error connecting to API: {ex.Message}");
            }
        }

        private void ExecuteSearch()
        {

            var filteredList = _allFetchedGames.Where(game =>
                (string.IsNullOrEmpty(SearchName) || game.Name.Contains(SearchName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(CitySearchText) || game.City.Contains(CitySearchText, StringComparison.OrdinalIgnoreCase)) &&
                game.Price <= SelectedMaximumPrice &&
                game.MinimumPlayerNumber >= SelectedMinimumPlayers
            ).ToList();

            _totalAvailableGamesCount = filteredList.Count;
            var paginatedList = filteredList
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();

            VisibleGames.Clear();
            foreach (var game in paginatedList)
            {
                VisibleGames.Add(game);
            }

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(NoResultsMessage));
        }

        private void ClearFilters()
        {
            SearchName = string.Empty;
            CitySearchText = string.Empty;
            SelectedMaximumPrice = DefaultMaximumPrice;
            SelectedMinimumPlayers = DefaultMinimumPlayers;
            CurrentPage = FirstPageNumber;

            ExecuteSearch();
        }

        public void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                ExecuteSearch();
            }
        }

        public void GoToPreviousPage()
        {
            if (CurrentPage > FirstPageNumber)
            {
                CurrentPage--;
                ExecuteSearch();
            }
        }

        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
