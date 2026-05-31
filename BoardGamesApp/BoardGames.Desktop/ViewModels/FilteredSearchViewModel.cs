
using BoardGames.Desktop.Commands;

namespace BoardGames.Desktop.ViewModels
{
    public class FilteredSearchViewModel : INotifyPropertyChanged
    {
        private const int ItemsPerPage = 10;
        private const int MinimumCharactersForCitySearch = 2;
        private const int FirstPage = 1;
        private const int MinimumPlayers = 0;
        private const double DefaultMaxPrice = 0;
        private const int MinimumPageNumber = 1;
        private const int NoGavesAvailable = 0;

        private readonly InterfaceSearchAndFilterService searchService;
        private readonly InterfaceGeographicalService geographicalService;

        private string citySearchText = string.Empty;
        private int currentPage = FirstPage;
        private GameDTO? selectedGame;
        private double selectedMaximumPrice;
        private double selectedMinimumPlayers;
        private DateTimeOffset? selectedStartDate;
        private DateTimeOffset? selectedEndDate;
        private string? selectedSortOption;
        private string locationError = string.Empty;
        public event Action<string>? OnErrorOccurred;
        public event Action<int>? OnGameSelectedRequest;
        public event Action? OnGoBackRequest;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<GameDTO> VisibleGames { get; set; } = new();
        public DateTimeOffset Today => DateTimeOffset.Now.Date;
        public FilterCriteria CurrentFilter { get; set; }
        public GameDTO[] BaseResults { get; private set; }
        public GameDTO[] DisplayedResults { get; private set; }
        public bool HasNoResults { get; private set; }
        public string NoResultsMessage => HasNoResults
            ? "No games isfound matching your criteria. Try adjusting your filters or search terms."
            : string.Empty;
        public List<GameDTO> Games { get; set; } = new();
        public GameDTO? SelectedGame
        {
            get => selectedGame;
            set
            {
                if (selectedGame != value)
                {
                    selectedGame = value;
                    OnPropertyChanged(nameof(SelectedGame));

                    if (selectedGame != null)
                    {
                        try
                        {
                            this.SelectGame(selectedGame.GameId);
                        }
                        catch (Exception ex)
                        {
                            RaiseError($"Could not select the game. {ex.Message}");
                        }
                        finally
                        {
                            selectedGame = null;
                            OnPropertyChanged(nameof(SelectedGame));
                        }
                    }
                }
            }
        }
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        public int TotalPages
        {
            get
            {
                if (Games == null || Games.Count == NoGavesAvailable)
                {
                    return FirstPage;
                }

                return (int)Math.Ceiling((double)Games.Count / ItemsPerPage);
            }
        }
        public double SelectedMaximumPrice
        {
            get => selectedMaximumPrice;
            set
            {
                selectedMaximumPrice = value;
                OnPropertyChanged(nameof(SelectedMaximumPrice));
            }
        }
        public double SelectedMinimumPlayers
        {
            get => selectedMinimumPlayers;
            set
            {
                selectedMinimumPlayers = value;
                OnPropertyChanged(nameof(SelectedMinimumPlayers));
            }
        }
        public DateTimeOffset MinimumEndDate => SelectedStartDate.HasValue
            ? SelectedStartDate.Value.AddDays(1)
            : Today;
        public DateTimeOffset? SelectedStartDate
        {
            get => selectedStartDate;
            set
            {
                selectedStartDate = value;
                OnPropertyChanged(nameof(SelectedStartDate));
                OnPropertyChanged(nameof(MinimumEndDate));

                if (SelectedEndDate.HasValue && value.HasValue && SelectedEndDate.Value <= value.Value)
                {
                    SelectedEndDate = null;
                }
            }
        }
        public DateTimeOffset MinimumStartDate => Today;
        public DateTimeOffset? SelectedEndDate
        {
            get => selectedEndDate;
            set
            {
                selectedEndDate = value;
                OnPropertyChanged(nameof(SelectedEndDate));
            }
        }
        public string? SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                if (selectedSortOption != value)
                {
                    selectedSortOption = value;
                    OnPropertyChanged(nameof(SelectedSortOption));
                    _ = ApplySortOnly();
                }
            }
        }
        public string LocationError
        {
            get => locationError;
            set
            {
                locationError = value;
                OnPropertyChanged(nameof(LocationError));
            }
        }
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SelectGameCommand { get; }
        public ICommand ApplySelectedUiFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand GoBackCommand { get; }
        public ObservableCollection<string> CitySuggestions { get; } = new();
        public FilteredSearchViewModel(InterfaceSearchAndFilterService searchService, InterfaceGeographicalService geographicalService)
        {
            this.geographicalService = geographicalService ?? throw new ArgumentNullException(nameof(geographicalService));
            this.searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));

            CurrentFilter = new FilterCriteria();
            BaseResults = Array.Empty<GameDTO>();
            DisplayedResults = Array.Empty<GameDTO>();
            HasNoResults = false;

            SelectedMaximumPrice = DefaultMaxPrice;
            SelectedStartDate = null;
            SelectedEndDate = null;

            SearchCommand = new RelayCommand(async _ => await SearchGamesByFilter(CurrentFilter));
            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            GoBackCommand = new RelayCommand(_ => GoBack());

            SelectGameCommand = new RelayCommand(selectedGameObject =>
            {
                try
                {
                    if (selectedGameObject is GameDTO selectedGameDto)
                    {
                        this.SelectGame(selectedGameDto.GameId);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError($"Could not open game details. {ex.Message}");
                }
            });

            ApplySelectedUiFiltersCommand = new RelayCommand(_ => ApplySelectedUiFilters());
            ClearFiltersCommand = new RelayCommand(_ => ClearAllFilters());
        }
        public async Task Initialize(FilterCriteria initialFilter)
        {
            try
            {
                if (initialFilter == null)
                {
                    throw new ArgumentNullException(nameof(initialFilter));
                }

                CurrentFilter = initialFilter;
                CitySearchText = CurrentFilter.City ?? string.Empty;

                if (CurrentFilter.AvailabilityRange != null)
                {
                    SelectedStartDate = new DateTimeOffset(CurrentFilter.AvailabilityRange.StartTime);
                    SelectedEndDate = new DateTimeOffset(CurrentFilter.AvailabilityRange.EndTime);
                }
                else
                {
                    SelectedStartDate = null;
                    SelectedEndDate = null;
                }

                await SearchGamesByFilter(CurrentFilter);
            }
            catch (Exception ex)
            {
                RaiseError($"Could not initialize search results. {ex.Message}");
            }
        }
        public async Task LoadSearchResults(FilterCriteria searchCriteria)
        {
            try
            {
                BaseResults = await searchService.SearchGamesByFilter(searchCriteria) ?? Array.Empty<GameDTO>();
                DisplayedResults = BaseResults;
                Games = DisplayedResults.ToList();
                CurrentPage = FirstPage;
                OnPropertyChanged(nameof(TotalPages));
                RefreshPage();
                UpdateNoResultsState();
            }
            catch (Exception ex)
            {
                BaseResults = Array.Empty<GameDTO>();
                DisplayedResults = Array.Empty<GameDTO>();
                Games = new List<GameDTO>();
                RefreshPage();
                UpdateNoResultsState();
                RaiseError($"Could not load search results. {ex.Message}");
            }
        }
        public void LoadDiscoveryResults(GameDTO[] discoveryResults)
        {
            try
            {
                BaseResults = discoveryResults ?? Array.Empty<GameDTO>();
                DisplayedResults = BaseResults;
                Games = DisplayedResults.ToList();
                CurrentPage = FirstPage;
                OnPropertyChanged(nameof(TotalPages));
                RefreshPage();
                UpdateNoResultsState();
            }
            catch (Exception ex)
            {
                BaseResults = Array.Empty<GameDTO>();
                DisplayedResults = Array.Empty<GameDTO>();
                Games = new List<GameDTO>();
                RefreshPage();
                UpdateNoResultsState();
                RaiseError($"Could not load discovery results. {ex.Message}");
            }
        }
        public async Task ApplyFilters()
        {
            try
            {
                if (!searchService.IsValidDateRange(
                    CurrentFilter.AvailabilityRange?.StartTime,
                    CurrentFilter.AvailabilityRange?.EndTime))
                {
                    RaiseError("Please select a valid date range.");
                    return;
                }

                DisplayedResults = await searchService.ApplyFilters(BaseResults, CurrentFilter) ?? Array.Empty<GameDTO>();
                Games = DisplayedResults.ToList();
                CurrentPage = FirstPage;
                OnPropertyChanged(nameof(TotalPages));
                RefreshPage();
                UpdateNoResultsState();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not apply filters. {ex.Message}");
            }
        }
        public async Task ApplySelectedUiFilters()
        {
            try
            {
                if (!searchService.IsValidPlayersCount((int?)SelectedMinimumPlayers) ||
                    !searchService.IsValidDateRange(
                        SelectedStartDate?.DateTime,
                        SelectedEndDate?.DateTime))
                {
                    RaiseError("Please enter valid filter values.");
                    return;
                }

                searchService.UpdateFilterFromUI(
                    CurrentFilter,
                    SelectedMaximumPrice,
                    SelectedMinimumPlayers,
                    SelectedStartDate?.DateTime,
                    SelectedEndDate?.DateTime);

                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not apply selected filters. {ex.Message}");
            }
        }
        public async Task RemoveNameFilter()
        {
            try
            {
                CurrentFilter.Name = null;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not remove name filter. {ex.Message}");
            }
        }
        public async Task RemoveCityFilter()
        {
            try
            {
                CurrentFilter.City = null;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not remove city filter. {ex.Message}");
            }
        }
        public async Task RemovePriceFilter()
        {
            try
            {
                CurrentFilter.MaximumPrice = null;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not remove price filter. {ex.Message}");
            }
        }
        public async Task RemovePlayersFilter()
        {
            try
            {
                CurrentFilter.PlayerCount = null;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not remove players filter. {ex.Message}");
            }
        }
        public async Task RemoveDateFilter()
        {
            try
            {
                CurrentFilter.AvailabilityRange = null;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not remove date filter. {ex.Message}");
            }
        }
        public async Task SetPriceAscendingSort()
        {
            try
            {
                CurrentFilter.SortOption = SortOption.PriceAscending;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not sort by ascending price. {ex.Message}");
            }
        }
        public async Task SetPriceDescendingSort()
        {
            try
            {
                CurrentFilter.SortOption = SortOption.PriceDescending;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not sort by descending price. {ex.Message}");
            }
        }
        public async Task ClearSorting()
        {
            try
            {
                CurrentFilter.SortOption = SortOption.None;
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not clear sorting. {ex.Message}");
            }
        }
        public void ClearAllFilters()
        {
            try
            {
                CurrentFilter.Reset();
                SelectedMaximumPrice = DefaultMaxPrice;
                SelectedMinimumPlayers = MinimumPlayers;
                SelectedSortOption = null;
                SelectedStartDate = null;
                SelectedEndDate = null;
                CitySearchText = string.Empty;
                LocationError = string.Empty;

                DisplayedResults = BaseResults;
                Games = DisplayedResults.ToList();
                CurrentPage = FirstPage;
                OnPropertyChanged(nameof(TotalPages));
                RefreshPage();
                UpdateNoResultsState();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not clear filters. {ex.Message}");
            }
        }
        public async Task ApplySortOnly()
        {
            try
            {
                if (SelectedSortOption == "Closest to me" && string.IsNullOrWhiteSpace(CitySearchText))
                {
                    LocationError = "Please enter a city to measure from.";
                    selectedSortOption = null;
                    OnPropertyChanged(nameof(SelectedSortOption));
                    return;
                }

                LocationError = string.Empty;

                CurrentFilter.SortOption = SelectedSortOption switch
                {
                    "Price: lowest to highest" => SortOption.PriceAscending,
                    "Price: highest to lowest" => SortOption.PriceDescending,
                    "Closest to me" => SortOption.Location,
                    _ => SortOption.None,
                };

                if (CurrentFilter.SortOption == SortOption.Location)
                {
                    await SearchGamesByFilter(CurrentFilter);
                }
                else
                {
                    await ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Could not apply sorting. {ex.Message}");
            }
        }
        public void SelectGame(int gameId)
        {
            try
            {
                OnGameSelectedRequest?.Invoke(gameId);
            }
            catch (Exception ex)
            {
                RaiseError($"Could not navigate to game details. {ex.Message}");
            }
        }
        public async Task SearchGamesByFilter(FilterCriteria filterCriteria)
        {
            try
            {
                if (filterCriteria == null)
                {
                    throw new ArgumentNullException(nameof(filterCriteria));
                }

                if (!searchService.IsValidDateRange(
                    SelectedStartDate?.DateTime,
                    SelectedEndDate?.DateTime))
                {
                    RaiseError("Please select a valid date range.");
                    return;
                }

                Games = (await searchService.SearchGamesByFilter(filterCriteria))?.ToList() ?? new List<GameDTO>();
                DisplayedResults = Games.ToArray();
                BaseResults = DisplayedResults;
                CurrentPage = FirstPage;
                OnPropertyChanged(nameof(TotalPages));
                RefreshPage();
                UpdateNoResultsState();
            }
            catch (Exception ex)
            {
                Games = new List<GameDTO>();
                DisplayedResults = Array.Empty<GameDTO>();
                BaseResults = Array.Empty<GameDTO>();
                RefreshPage();
                UpdateNoResultsState();
                RaiseError($"Search failed. {ex.Message}");
            }
        }
        public string CitySearchText
        {
            get => citySearchText;
            set
            {
                if (citySearchText != value)
                {
                    citySearchText = value;
                    OnPropertyChanged(nameof(CitySearchText));
                    CurrentFilter.City = value;
                    UpdateCitySuggestions(value);
                }
            }
        }
        public void NextPage()
        {
            try
            {
                if (CurrentPage * ItemsPerPage < Games.Count)
                {
                    CurrentPage++;
                    RefreshPage();
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Could not go to next page. {ex.Message}");
            }
        }
        public void PreviousPage()
        {
            try
            {
                if (CurrentPage > MinimumPageNumber)
                {
                    CurrentPage--;
                    RefreshPage();
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Could not go to previous page. {ex.Message}");
            }
        }
        protected void OnPropertyChanged(string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void UpdateNoResultsState()
        {
            HasNoResults = DisplayedResults.Length == 0;
            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(NoResultsMessage));
        }
        private void RefreshPage()
        {
            try
            {
                VisibleGames.Clear();

                var pageListings = Games
                    .Skip((CurrentPage - 1) * ItemsPerPage)
                    .Take(ItemsPerPage)
                    .ToList();

                foreach (var gameItem in pageListings)
                {
                    VisibleGames.Add(gameItem);
                }
                OnPropertyChanged(nameof(VisibleGames));
            }
            catch (Exception ex)
            {
                RaiseError($"Could not refresh the page. {ex.Message}");
            }
        }
        private void GoBack()
        {
            try
            {
                OnGoBackRequest?.Invoke();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not go back. {ex.Message}");
            }
        }
        private void UpdateCitySuggestions(string input)
        {
            try
            {
                CitySuggestions.Clear();

                if (!string.IsNullOrWhiteSpace(input) && input.Length >= MinimumCharactersForCitySearch)
                {
                    var cityMatches = geographicalService.GetCitySuggestions(input);
                    foreach (var city in cityMatches)
                    {
                        CitySuggestions.Add(city);
                    }
                }
            }
            catch (Exception ex)
            {
                CitySuggestions.Clear();
                RaiseError($"Could not load city suggestions. {ex.Message}");
            }
        }
        private void RaiseError(string message) => OnErrorOccurred?.Invoke(message);
    }
}
