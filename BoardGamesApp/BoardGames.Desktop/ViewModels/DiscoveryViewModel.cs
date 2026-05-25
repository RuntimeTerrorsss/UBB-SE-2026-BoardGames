// <copyright file="DiscoveryViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Desktop.Commands;

namespace BoardGames.Desktop.ViewModels
{
    /// <summary>
    /// Provides the logic for discovering and filtering games, including pagination and search capabilities.
    /// </summary>
    public class DiscoveryViewModel : INotifyPropertyChanged
    {
        private const int MinimumCitySearchLength = 2;
        private const int ItemsPerPage = 10;
        private const int FirstDayOfMonth = 1;
        private const int MidnightHour = 0;
        private const int MidnightMinute = 0;
        private const int MidnightSecond = 0;
        private const int NoPagesAvailable = 0;
        private const int NoGamesAvailable = 0;
        private const int InitialPage = 1;
        private readonly InterfaceSearchAndFilterService searchAndFilterService;
        private readonly InterfaceGeographicalService geographicalService;
        private DateTimeOffset? selectedEndDate;
        private int currentPage = 1;
        private int totalAvailableGamesCount;
        private string citySearchText = string.Empty;
        private bool showOthersHeader;
        private DateTimeOffset? selectedStartDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryViewModel"/> class.
        /// </summary>
        /// <param name="searchService">The service used for searching and filtering games.</param>
        /// <param name="geographicalService">The service used for geographical location suggestions.</param>
        public DiscoveryViewModel(InterfaceSearchAndFilterService searchService, InterfaceGeographicalService geographicalService)
        {
            searchAndFilterService = searchService;
            this.geographicalService = geographicalService;

            var today = DateTimeOffset.Now.Date;
            selectedStartDate = today;
            selectedEndDate = today;

            NextPageCommand = new RelayCommand(_ => GoToNextPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage());
            SearchCommand = new RelayCommand(_ => SearchGamesByFilter(Filter));

            SessionContext.GetInstance().OnUserChanged += this.LoadPaginatedDiscoveryFeed;

            try
            {
                LoadPaginatedDiscoveryFeed();
            }
            catch (Exception exception)
            {
                OnErrorOccurred?.Invoke($"Could not load discovery feed. {exception.Message}");
            }
        }

        /// <summary>
        /// Occurs when the current page of the discovery feed changes.
        /// </summary>
        public event Action? OnPageChanged;

        /// <summary>
        /// Occurs when a request is made to view the details of a specific game.
        /// </summary>
        public event Action<int>? OnGameSelectedRequest;

        /// <summary>
        /// Occurs when a search request is initiated with specific filter criteria.
        /// </summary>
        public event Action<FilterCriteria>? OnSearchRequest;

        /// <summary>
        /// Occurs when an error is encountered during discovery operations.
        /// </summary>
        public event Action<string>? OnErrorOccurred;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the list of games available for rent tonight.
        /// </summary>
        public List<GameDTO> AvailableTonightGames { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of other available games that do not match the "tonight" criteria.
        /// </summary>
        public List<GameDTO> OtherAvailableGames { get; set; } = new();

        /// <summary>
        /// Gets a value indicating whether the end date selection is enabled based on the start date.
        /// </summary>
        public bool IsEndDateEnabled => SelectedStartDate.HasValue;

        private int TotalGamesCount => totalAvailableGamesCount;

        /// <summary>
        /// Gets or sets a value indicating whether the header for "Other games" should be visible.
        /// </summary>
        public bool ShowOthersHeader
        {
            get => showOthersHeader;
            set
            {
                showOthersHeader = value;
                OnPropertyChanged(nameof(ShowOthersHeader));
            }
        }

        /// <summary>
        /// Gets or sets the filter criteria used for searching games.
        /// </summary>
        public FilterCriteria Filter { get; set; } = new();

        /// <summary>
        /// Gets the minimum allowed start date for a search, typically the current date.
        /// </summary>
        public DateTimeOffset MinStartDate => DateTimeOffset.Now.Date;

        /// <summary>
        /// Gets the minimum allowed end date based on the currently selected start date.
        /// </summary>
        public DateTimeOffset MinEndDate =>
        SelectedStartDate.HasValue
        ? new DateTimeOffset(
            SelectedStartDate.Value.Year,
            SelectedStartDate.Value.Month,
            FirstDayOfMonth,
            MidnightHour,
            MidnightMinute,
            MidnightSecond,
            TimeSpan.Zero)
        : DateTimeOffset.Now.Date;

        /// <summary>
        /// Gets or sets the selected start date for the game search.
        /// </summary>
        public DateTimeOffset? SelectedStartDate
        {
            get => selectedStartDate;
            set
            {
                var newValue = value?.Date;

                if (!newValue.HasValue)
                {
                    newValue = DateTimeOffset.Now.Date;
                }

                if (selectedStartDate != newValue)
                {
                    selectedStartDate = newValue;
                    OnPropertyChanged(nameof(SelectedStartDate));
                    OnPropertyChanged(nameof(MinEndDate));
                    OnPropertyChanged(nameof(IsEndDateEnabled));

                    selectedEndDate = selectedStartDate;

                    OnPropertyChanged(nameof(SelectedEndDate));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected end date for the game search.
        /// </summary>
        public DateTimeOffset? SelectedEndDate
        {
            get => selectedEndDate;
            set
            {
                var newValue = value?.Date;

                if (!newValue.HasValue)
                {
                    newValue = SelectedStartDate?.Date ?? DateTimeOffset.Now.Date;
                }

                if (SelectedStartDate.HasValue && newValue.HasValue &&
                    newValue.Value < SelectedStartDate.Value)
                {
                    newValue = SelectedStartDate.Value.Date;
                }

                selectedEndDate = newValue;
                OnPropertyChanged(nameof(SelectedEndDate));
            }
        }

        /// <summary>
        /// Gets or sets the current page number in the discovery feed.
        /// </summary>
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        /// <summary>
        /// Gets the total number of pages available based on the total games count.
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (TotalGamesCount == 0)
                {
                    return NoPagesAvailable;
                }

                return (int)Math.Ceiling((double)TotalGamesCount / ItemsPerPage);
            }
        }

        /// <summary>
        /// Gets or sets the text used to search for games in a specific city.
        /// </summary>
        public string CitySearchText
        {
            get => citySearchText;
            set
            {
                if (citySearchText != value)
                {
                    citySearchText = value;
                    OnPropertyChanged(nameof(CitySearchText));
                    Filter.City = value;
                    UpdateCitySuggestions(value);
                }
            }
        }

        /// <summary>
        /// Gets the collection of city suggestions based on the current search text.
        /// </summary>
        public ObservableCollection<string> CitySuggestions { get; } = new();

        /// <summary>
        /// Gets the command to navigate to the next page.
        /// </summary>
        public ICommand NextPageCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the previous page.
        /// </summary>
        public ICommand PreviousPageCommand { get; }

        /// <summary>
        /// Gets the command to perform a search based on current filters.
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// Gets the message to be displayed when no games match the discovery or search criteria.
        /// </summary>
        public string NoResultsMessage => TotalGamesCount == NoGamesAvailable ? "No games available." : string.Empty;

        /// <summary>
        /// Loads paginated discovery feed and updates UI properties.
        /// </summary>
        public async void LoadPaginatedDiscoveryFeed()
        {
            try
            {
                int currentUserId = SessionContext.GetInstance().UserId;

                var discoveryFeedResult = await searchAndFilterService.GetDiscoveryFeedPaged(currentUserId, CurrentPage, ItemsPerPage);

                AvailableTonightGames = discoveryFeedResult.AvailableTonight;
                OtherAvailableGames = discoveryFeedResult.Others;
                ShowOthersHeader = OtherAvailableGames.Any();
                totalAvailableGamesCount = discoveryFeedResult.TotalAvailableGamesCount;

                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(AvailableTonightGames));
                OnPropertyChanged(nameof(OtherAvailableGames));
                OnPropertyChanged(nameof(NoResultsMessage));
            }
            catch (Exception exception)
            {
                OnErrorOccurred?.Invoke($"Could not load discovery feed. {exception.Message}");
            }
        }

        /// <summary>
        /// Navigates to the next page in the discovery feed.
        /// </summary>
        public void GoToNextPage()
        {
            try
            {
                if (CurrentPage * ItemsPerPage < TotalGamesCount)
                {
                    CurrentPage++;
                    LoadPaginatedDiscoveryFeed();
                    OnPageChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not go to next page. {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates to the previous page in the discovery feed.
        /// </summary>
        public void GoToPreviousPage()
        {
            try
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    LoadPaginatedDiscoveryFeed();
                    OnPageChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not go to previous page. {ex.Message}");
            }
        }

        /// <summary>
        /// Searches for games based on the provided filter criteria and updates the discovery feed.
        /// </summary>
        /// <param name="criteria">The filter criteria containing search parameters like name, city, and price.</param>
        public void SearchGamesByFilter(FilterCriteria criteria)
        {
            try
            {
                if (!searchAndFilterService.IsValidDateRange(
                    SelectedStartDate?.DateTime,
                    SelectedEndDate?.DateTime))
                {
                    return;
                }

                Filter.Name = criteria.Name;
                Filter.City = criteria.City;
                Filter.SortOption = criteria.SortOption;
                Filter.MaximumPrice = criteria.MaximumPrice;
                Filter.PlayerCount = criteria.PlayerCount;
                Filter.UserId = SessionContext.GetInstance().UserId;

                UpdateAvailabilityRange();

                CurrentPage = InitialPage;
                OnSearchRequest?.Invoke(Filter);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not perform search. {ex.Message}");
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. This is optional because of CallerMemberName.</param>
        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateAvailabilityRange()
        {
            try
            {
                if (SelectedStartDate.HasValue &&
                    SelectedEndDate.HasValue &&
                    SelectedStartDate.Value <= SelectedEndDate.Value)
                {
                    Filter.AvailabilityRange = new TimeRange(
                        SelectedStartDate.Value.Date,
                        SelectedEndDate.Value.Date);
                }
                else
                {
                    Filter.AvailabilityRange = null;
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not update availability range. {ex.Message}");
            }
        }

        private void UpdateCitySuggestions(string input)
        {
            try
            {
                CitySuggestions.Clear();

                if (!string.IsNullOrWhiteSpace(input) && input.Length >= MinimumCitySearchLength)
                {
                    var matches = geographicalService.GetCitySuggestions(input);
                    foreach (var match in matches)
                    {
                        CitySuggestions.Add(match);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not load city suggestions. {ex.Message}");
            }
        }
    }
}
