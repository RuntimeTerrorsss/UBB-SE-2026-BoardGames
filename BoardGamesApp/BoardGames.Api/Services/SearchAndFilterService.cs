// <copyright file="SearchAndFilterService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using BoardGames.Api.Mappers;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    /// <summary>
    /// Service responsible for searching, filtering, and retrieving game feeds.
    /// </summary>
    public class SearchAndFilterService : InterfaceSearchAndFilterService
    {
        private const int MinimumAllowedPlayers = 0;
        private const double MinimumFilterValue = 0;
        private readonly InterfaceGamesRepository gamesRepository;
        private readonly IUserRepository usersRepository;
        private readonly IRentalRepository rentalsRepository;
        private readonly InterfaceGeographicalService geographicalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchAndFilterService"/> class.
        /// Initializes a new instance.
        /// </summary>
        /// <param name="gamesRepository">The repository for game data operations.</param>
        /// <param name="usersRepository">The repository for user data operations.</param>
        /// <param name="rentalsRepository">The repository for rental data operations.</param>
        /// <param name="geographicalService">The service for geographical and location-based calculations.</param>
        public SearchAndFilterService(InterfaceGamesRepository gamesRepository, IUserRepository usersRepository, IRentalRepository rentalsRepository, InterfaceGeographicalService geographicalService)
        {
            this.gamesRepository = gamesRepository;
            this.usersRepository = usersRepository;
            this.rentalsRepository = rentalsRepository;
            this.geographicalService = geographicalService;
        }

        /// <summary>
        /// Searches for games based on the provided filter criteria.
        /// </summary>
        /// <param name="filter">The criteria to filter games.</param>
        /// <returns>An array of <see cref="GameDTO"/> matching the criteria.</returns>
        public async Task<GameDTO[]> SearchGamesByFilter(FilterCriteria filter)
        {
            try
            {
                string? originalFilterCity = filter.City;
                filter.City = null;

                var filteredGamesFromRepository = await this.gamesRepository.GetGamesByFilter(filter);
                filter.City = originalFilterCity;

                var filteredGamesResult = new List<GameDTO>();
                var cachedOwnersById = new Dictionary<int, User>();

                foreach (var filteredGame in filteredGamesFromRepository)
                {
                    if (!cachedOwnersById.TryGetValue(filteredGame.OwnerId, out var cachedOwnerGame))
                    {
                        cachedOwnerGame = await this.usersRepository.GetGameById(filteredGame.OwnerId);

                        if (cachedOwnerGame != null)
                        {
                            cachedOwnersById[filteredGame.OwnerId] = cachedOwnerGame;
                        }
                    }

                    var gameOwner = cachedOwnersById[filteredGame.OwnerId];

                    var gameDTO = new GameDTO
                    {
                        GameId = filteredGame.Id,
                        Name = filteredGame.Name,
                        Image = GameImageMapper.GetImageUrl(filteredGame.Name),
                        Price = filteredGame.PricePerDay,
                        City = gameOwner != null ? gameOwner.City : string.Empty,
                        MaximumPlayerNumber = filteredGame.MaximumPlayerNumber,
                        MinimumPlayerNumber = filteredGame.MinimumPlayerNumber,
                    };

                    filteredGamesResult.Add(gameDTO);
                }

                GameDTO[] filteredGamesArray = filteredGamesResult.ToArray();

                //// sorting by distance

                //// this is if we decide to only use this methode and remove the ApplyFilters method
                //// only runs this code if SortOption is set, so never from feed

                return await this.ApplyFilters(filteredGamesArray, filter);
            }
            catch (Exception thrownException)
            {
                throw new InvalidOperationException("Failed to search for games.", thrownException);
            }
        }

        /// <summary>
        /// Retrieves a feed of games available tonight for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the feed or null.</param>
        /// <returns>An array of <see cref="GameDTO"/> available tonight.</returns>
        public async Task<GameDTO[]> GetGamesFeedAvailableTonightByUser(int userId)
        {
            try
            {
                var availableTonightGameList = await this.gamesRepository.GetGamesForFeedAvailableTonight(userId);
                var availableTonightGamesResult = new List<GameDTO>();

                foreach (var availableTonightGame in availableTonightGameList)
                {
                    var gameOwner = await this.usersRepository.GetGameById(availableTonightGame.OwnerId);

                    if (gameOwner != null)
                    {
                        var gameDTO = this.MapToGameDTO(availableTonightGame, gameOwner);
                        availableTonightGamesResult.Add(gameDTO);
                    }
                }

                return availableTonightGamesResult.ToArray();
            }
            catch (Exception thrownException)
            {
                throw new InvalidOperationException("Failed to retrieve <<Available tonight>> feed.", thrownException);
            }
        }

        /// <summary>
        /// Retrieves a feed of other relevant games for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the feed or null.</param>
        /// <returns>An array of <see cref="GameDTO"/> representing other games.</returns>
        public async Task<GameDTO[]> GetOtherGamesFeedByUser(int userId)
        {
            try
            {
                var otherFeedGames = await this.gamesRepository.GetRemainingGamesForFeed(userId);
                var otherFeedGamesResult = new List<GameDTO>();
                foreach (var otherFeedGame in otherFeedGames)
                {
                    var gameOwner = await this.usersRepository.GetGameById(otherFeedGame.OwnerId);

                    if (gameOwner == null)
                    {
                        continue;
                    }

                    var gameDTO = this.MapToGameDTO(otherFeedGame, gameOwner);
                    otherFeedGamesResult.Add(gameDTO);
                }

                return otherFeedGamesResult.ToArray();
            }
            catch (Exception thrownException)
            {
                throw new InvalidOperationException("Failed to retrieve <<Others>> feed.", thrownException);
            }
        }

        /// <summary>
        /// Filters and sorts an array of games based on the provided criteria, including name, price, player count, and location.
        /// </summary>
        /// <param name="initialGamesCollection">The initial collection of games to be filtered.</param>
        /// <param name="activeFilter">The criteria used for filtering and sorting the games.</param>
        /// <returns>An array of <see cref="GameDTO"/> objects that match the filter criteria.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs during the filtering process.</exception>
        public async Task<GameDTO[]> ApplyFilters(GameDTO[] initialGamesCollection, FilterCriteria activeFilter)
        {
            try
            {
                IEnumerable<GameDTO> filteredGames = initialGamesCollection;
                var resolvedCityName = this.ResolveCityName(activeFilter.City);
                var normalizedFilterCity = this.NormalizeCityName(resolvedCityName);

                if (!string.IsNullOrWhiteSpace(activeFilter.Name))
                {
                    filteredGames = filteredGames.Where(filteredGame =>
                        filteredGame.Name.Contains(activeFilter.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (activeFilter.MaximumPrice.HasValue)
                {
                    filteredGames = filteredGames.Where(filteredGame =>
                        filteredGame.Price <= activeFilter.MaximumPrice.Value);
                }

                if (activeFilter.PlayerCount.HasValue)
                {
                    filteredGames = filteredGames.Where(filteredGame =>
                        filteredGame.MaximumPlayerNumber >= activeFilter.PlayerCount.Value);
                }

                if (!string.IsNullOrWhiteSpace(normalizedFilterCity) &&
                    activeFilter.SortOption != SortOption.Location)
                {
                    filteredGames = filteredGames.Where(filteredGame =>
                        !string.IsNullOrWhiteSpace(filteredGame.City) &&
                        this.NormalizeCityName(filteredGame.City).Contains(normalizedFilterCity, StringComparison.OrdinalIgnoreCase));
                }

                switch (activeFilter.SortOption)
                {
                    case SortOption.PriceAscending:
                        filteredGames = filteredGames.OrderBy(filteredGame => filteredGame.Price);
                        break;

                    case SortOption.PriceDescending:
                        filteredGames = filteredGames.OrderByDescending(filteredGame => filteredGame.Price);
                        break;

                    case SortOption.Location:
                        if (!string.IsNullOrWhiteSpace(resolvedCityName))
                        {
                            var userCityDetails =
                                this.geographicalService.GetCityDetails(resolvedCityName);

                            if (userCityDetails.IsFound)
                            {
                                var cachedCityDistanceLookup = new Dictionary<string, double?>();

                                filteredGames = filteredGames.OrderBy(filteredGame =>
                                {
                                    if (string.IsNullOrWhiteSpace(filteredGame.City))
                                    {
                                        return double.MaxValue;
                                    }

                                    if (!cachedCityDistanceLookup.TryGetValue(filteredGame.City, out double? cachedDistance))
                                    {
                                        var gameCityDetails =
                                            this.geographicalService.GetCityDetails(filteredGame.City);

                                        cachedDistance = gameCityDetails.IsFound
                                            ? GeographicDistance.CalculateDistance(
                                                userCityDetails.Latitude,
                                                userCityDetails.Longitude,
                                                gameCityDetails.Latitude,
                                                gameCityDetails.Longitude)
                                            : null;

                                        cachedCityDistanceLookup[filteredGame.City] = cachedDistance;
                                    }

                                    return cachedDistance ?? double.MaxValue;
                                });
                            }
                        }

                        break;

                    case SortOption.None:
                    default:
                        break;
                }

                if (activeFilter.AvailabilityRange != null)
                {
                    var tasks = filteredGames.Select(async game => new
                    {
                        Game = game,
                        IsAvailable = await this.rentalsRepository.CheckGameAvailability(activeFilter.AvailabilityRange.StartTime, activeFilter.AvailabilityRange.EndTime, game.GameId),
                    });

                    var results = await Task.WhenAll(tasks);

                    filteredGames = results.Where(filteredGame => filteredGame.IsAvailable).Select(filteredGame => filteredGame.Game);
                }

                return filteredGames.ToArray();
            }
            catch (Exception thrownException)
            {
                throw new InvalidOperationException("Failed to apply filters.", thrownException);
            }
        }

        /// <summary>
        /// Retrieves a paginated discovery feed, splitting games into those available tonight and others.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the feed is generated.</param>
        /// <param name="page">The current page number (1-based).</param>
        /// <param name="pageSize">The number of items to include per page.</param>
        /// <returns>A tuple containing available games for tonight, other available games, and the total count of games.</returns>
        public async Task<(List<GameDTO> AvailableTonight, List<GameDTO> Others, int TotalAvailableGamesCount)>
            GetDiscoveryFeedPaged(int userId, int page, int pageSize)
        {
            var availableTonightGameList = await this.GetGamesFeedAvailableTonightByUser(userId);
            var otherGameList = await this.GetOtherGamesFeedByUser(userId);

            var allDescoveryFeedGames = availableTonightGameList.Concat(otherGameList).DistinctBy(game => game.GameId).ToList();
            var totalAvailableGamesCount = allDescoveryFeedGames.Count;

            var paginatedGames = allDescoveryFeedGames
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedAvailableTonightGames = paginatedGames
                .Where(availableTonightGame => availableTonightGameList.Any(anyGameAvailbaleTonight => anyGameAvailbaleTonight.GameId == availableTonightGame.GameId))
                .ToList();

            var pagedOtherGames = paginatedGames
                .Where(otherGame => otherGameList.Any(anyOtherGame => anyOtherGame.GameId == otherGame.GameId))
                .ToList();

            return (pagedAvailableTonightGames, pagedOtherGames, totalAvailableGamesCount);
        }

        /// <summary>
        /// Validates if a given date range is logical (start date is before or equal to end date).
        /// </summary>
        /// <param name="requestedStartDate">The start date of the range.</param>
        /// <param name="requestedEndDate">The end date of the range.</param>
        /// <returns>True if the range is valid or both dates are null; false if only one date is provided or start is after end.</returns>
        public bool IsValidDateRange(DateTime? requestedStartDate, DateTime? requestedEndDate)
        {
            if (!requestedStartDate.HasValue && !requestedEndDate.HasValue)
            {
                return true;
            }

            if (!requestedStartDate.HasValue || !requestedEndDate.HasValue)
            {
                return false;
            }

            return requestedStartDate.Value <= requestedEndDate.Value;
        }

        /// <summary>
        /// checks if the number of players if valid.
        /// </summary>
        /// <param name="playersNumber">number of players.</param>
        /// <returns>true if the value is valid, false otherwise.</returns>
        public bool IsValidPlayersCount(int? playersNumber)
        {
            if (!playersNumber.HasValue)
            {
                return true;
            }

            return playersNumber.Value >= MinimumAllowedPlayers;
        }

        /// <summary>
        /// Updates the filter criteria object with values provided from the user interface.
        /// </summary>
        /// <param name="targetFilter">The filter object to be updated.</param>
        /// <param name="selectedMaximumPrice">The maximum price selected by the user.</param>
        /// <param name="selectedMinimumPlayerCount">The minimum number of players selected by the user.</param>
        /// <param name="selectedStartDate">The start date for availability.</param>
        /// <param name="selectedEndDate">The end date for availability.</param>
        public void UpdateFilterFromUI(FilterCriteria targetFilter, double selectedMaximumPrice, double selectedMinimumPlayerCount, DateTime? selectedStartDate, DateTime? selectedEndDate)
        {
            targetFilter.MaximumPrice = selectedMaximumPrice > MinimumFilterValue
                ? (decimal?)selectedMaximumPrice
                : null;

            targetFilter.PlayerCount = selectedMinimumPlayerCount > MinimumFilterValue
                ? (int?)selectedMinimumPlayerCount
                : null;

            if (this.IsValidDateRange(selectedStartDate, selectedEndDate))
            {
                if (selectedStartDate.HasValue && selectedEndDate.HasValue)
                {
                    targetFilter.AvailabilityRange = new TimeRange(
                        selectedStartDate.Value,
                        selectedEndDate.Value);
                }
                else
                {
                    targetFilter.AvailabilityRange = null;
                }
            }
            else
            {
                targetFilter.AvailabilityRange = null;
            }
        }

        /// <summary>
        /// Maps a <see cref="Game""")/>> entity and its owner's information to a <see cref="GameDTO""")/>>.
        /// </summary>
        /// <param name="gameEntity">The game entity to map.</param>
        /// <param name="gameOwnerEntity">The user who owns the game.</param>
        /// <returns>A data transfer object representing the game.</returns>
        private GameDTO MapToGameDTO(Game gameEntity, User? gameOwnerEntity)
        {
            return new GameDTO
            {
                GameId = gameEntity.Id,
                Name = gameEntity.Name,
                Image = GameImageMapper.GetImageUrl(gameEntity.Name),
                Price = gameEntity.PricePerDay,
                City = gameOwnerEntity?.City ?? string.Empty,
                MaximumPlayerNumber = gameEntity.MaximumPlayerNumber,
                MinimumPlayerNumber = gameEntity.MinimumPlayerNumber,
            };
        }

        private string? ResolveCityName(string? cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return cityName;
            }

            var cityDetails = this.geographicalService.GetCityDetails(cityName);
            return cityDetails.IsFound ? cityDetails.CityName : cityName;
        }

        private string NormalizeCityName(string? cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return string.Empty;
            }

            var normalized = cityName.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            var result = builder.ToString()
                .Normalize(NormalizationForm.FormC)
                .Trim()
                .ToLower()
                .Replace("-", " ");

            if (result == "bucharest")
            {
                return "bucuresti";
            }

            return result;
        }
    }
}
