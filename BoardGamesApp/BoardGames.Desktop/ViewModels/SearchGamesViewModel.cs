// <copyright file="SearchGamesViewModel.cs" company="BoardRent">
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
    public partial class SearchGamesViewModel : BaseViewModel
    {
        private const double NoMaximumPriceFilter = 0;
        private const double NoPlayerCountFilter = 0;
        private const int SingleGameCount = 1;

        private readonly IGameService gameService;
        private readonly ISessionContext sessionContext;
        private readonly Uri apiBaseUri;

        public SearchGamesViewModel(IGameService gameService, ISessionContext sessionContext)
            : this(gameService, sessionContext, null)
        {
        }

        public SearchGamesViewModel(IGameService gameService, ISessionContext sessionContext, Uri? apiBaseUriOverride)
        {
            this.gameService = gameService;
            this.sessionContext = sessionContext;
            apiBaseUri = apiBaseUriOverride ?? ResolveApiBaseUri();
        }

        public ObservableCollection<SearchGameCardViewModel> Games { get; } = new();

        public IReadOnlyList<string> SortOptions { get; } = new[]
        {
            "None",
            "PriceAscending",
            "PriceDescending",
        };

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string city = string.Empty;

        [ObservableProperty]
        private double maximumPrice;

        [ObservableProperty]
        private double playerCount;

        [ObservableProperty]
        private DateTimeOffset? availableFrom;

        [ObservableProperty]
        private DateTimeOffset? availableTo;

        [ObservableProperty]
        private string selectedSortOption = "None";

        [ObservableProperty]
        private string resultsSummary = string.Empty;

        [ObservableProperty]
        private string emptyStateMessage = string.Empty;

        public Action? OnNavigateToLogin { get; set; }

        public Visibility LoginButtonVisibility => sessionContext.IsLoggedIn ? Visibility.Collapsed : Visibility.Visible;

        public async Task LoadAsync()
        {
            await LoadGamesAsync(HasFiltersApplied());
        }

        [RelayCommand]
        private Task SearchAsync()
        {
            return LoadGamesAsync(HasFiltersApplied());
        }

        [RelayCommand]
        private async Task ClearAsync()
        {
            SearchText = string.Empty;
            City = string.Empty;
            MaximumPrice = NoMaximumPriceFilter;
            PlayerCount = NoPlayerCountFilter;
            AvailableFrom = null;
            AvailableTo = null;
            SelectedSortOption = "None";
            ErrorMessage = string.Empty;

            await LoadGamesAsync(useSearch: false);
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            OnNavigateToLogin?.Invoke();
        }

        private async Task LoadGamesAsync(bool useSearch)
        {
            ErrorMessage = string.Empty;

            if (AvailableFrom.HasValue && AvailableTo.HasValue && AvailableTo.Value.Date < AvailableFrom.Value.Date)
            {
                ErrorMessage = "End date cannot be earlier than the start date.";
                return;
            }

            IsLoading = true;

            try
            {
                ServiceResult<IReadOnlyList<GameSummaryDTO>> result = useSearch
                    ? await gameService.SearchGamesAsync(BuildSearchCriteria())
                    : await gameService.GetAllGamesAsync();

                if (!result.Success)
                {
                    Games.Clear();
                    ResultsSummary = string.Empty;
                    EmptyStateMessage = string.Empty;
                    ErrorMessage = result.Error ?? "Could not load games right now.";
                    return;
                }

                ApplyGames(result.Data ?? Array.Empty<GameSummaryDTO>());
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyGames(IEnumerable<GameSummaryDTO> games)
        {
            Games.Clear();

            foreach (var game in games)
            {
                Games.Add(new SearchGameCardViewModel(game, apiBaseUri));
            }

            ResultsSummary = Games.Count switch
            {
                0 => "No games matched the current search.",
                SingleGameCount => "1 game available",
                _ => $"{Games.Count} games available",
            };

            EmptyStateMessage = Games.Count == 0
                ? "Try clearing a filter or broadening your search."
                : string.Empty;
        }

        private GameSearchCriteriaDTO BuildSearchCriteria()
        {
            return new GameSearchCriteriaDTO
            {
                Name = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
                City = string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
                MaximumPrice = MaximumPrice > NoMaximumPriceFilter ? decimal.Round((decimal)MaximumPrice, 2) : null,
                PlayerCount = PlayerCount > NoPlayerCountFilter ? (int)Math.Ceiling(PlayerCount) : null,
                AvailableFrom = AvailableFrom?.Date,
                AvailableTo = AvailableTo?.Date,
                SortBy = SelectedSortOption == "None" ? null : SelectedSortOption,
            };
        }

        private bool HasFiltersApplied()
        {
            return !string.IsNullOrWhiteSpace(SearchText)
                || !string.IsNullOrWhiteSpace(City)
                || MaximumPrice > NoMaximumPriceFilter
                || PlayerCount > NoPlayerCountFilter
                || AvailableFrom.HasValue
                || AvailableTo.HasValue
                || SelectedSortOption != "None";
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

    public sealed class SearchGameCardViewModel
    {
        public SearchGameCardViewModel(GameSummaryDTO game, Uri apiBaseUri)
        {
            Id = game.Id;
            Name = game.Name;
            City = game.City;
            OwnerDisplayName = string.IsNullOrWhiteSpace(game.OwnerDisplayName) ? "BoardGames host" : game.OwnerDisplayName;
            PriceText = $"{game.Price:0.##} RON / day";
            PlayerRangeText = $"{game.MinimumPlayerNumber} - {game.MaximumPlayerNumber} players";
            GameImage = CreateImage(game.ImageUrl, apiBaseUri);
        }

        public int Id { get; }

        public string Name { get; }

        public string City { get; }

        public string OwnerDisplayName { get; }

        public string PriceText { get; }

        public string PlayerRangeText { get; }

        public BitmapImage? GameImage { get; }

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
    }
}
