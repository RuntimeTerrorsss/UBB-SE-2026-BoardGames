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
        private readonly IGameService _gameService;

        // Use the new DTO from Shared
        private ObservableCollection<GameSummaryDTO> _allFetchedGames = new();
        private ObservableCollection<GameSummaryDTO> _visibleGames = new();

        private int _currentPage = 1;
        private int _totalAvailableGamesCount;
        private string _citySearchText = string.Empty;

        // Filter properties (replacing the old FilterCriteria object)
        private string _searchName;
        private int _selectedMinimumPlayers = 1;
        private int _selectedMaximumPrice = 100;

        public DiscoveryViewModel(IGameService gameService)
        {
            _gameService = gameService;

            NextPageCommand = new RelayCommand(_ => GoToNextPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage());
            SearchCommand = new RelayCommand(_ => ExecuteSearch());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            // Load initial data
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
                // Task 9 rule: Load active games from the unified API
                var result = await _gameService.GetAllGamesAsync();

                if (result.IsSuccess)
                {
                    _allFetchedGames.Clear();
                    foreach (var game in result.Data)
                    {
                        _allFetchedGames.Add(game);
                    }

                    ExecuteSearch(); // Apply default filters and pagination
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
            // Note for Task 10/Shared Owner: 
            // We are filtering locally because POST api/games/search is missing from IGameService.

            var filteredList = _allFetchedGames.Where(g =>
                (string.IsNullOrEmpty(SearchName) || g.Name.Contains(SearchName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(CitySearchText) || g.City.Contains(CitySearchText, StringComparison.OrdinalIgnoreCase)) &&
                g.Price <= SelectedMaximumPrice &&
                g.MinimumPlayerNumber >= SelectedMinimumPlayers
            ).ToList();

            _totalAvailableGamesCount = filteredList.Count;

            // Client-side pagination logic
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
            SelectedMaximumPrice = 100;
            SelectedMinimumPlayers = 1;
            CurrentPage = 1;

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
            if (CurrentPage > 1)
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