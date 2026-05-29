using System.Globalization;
using System.Text;
using BoardGames.Api.Mappers;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services
{
    public class SearchAndFilterService : InterfaceSearchAndFilterService
    {
        private const int MinimumAllowedPlayers = 0;
        private const double MinimumFilterValue = 0;
        private readonly InterfaceGamesRepository gamesRepository;
        private readonly IUserRepository usersRepository;
        private readonly IRentalRepository rentalsRepository;
        private readonly InterfaceGeographicalService geographicalService;
        public SearchAndFilterService(InterfaceGamesRepository gamesRepository, IUserRepository usersRepository, IRentalRepository rentalsRepository, InterfaceGeographicalService geographicalService)
        {
            this.gamesRepository = gamesRepository;
            this.usersRepository = usersRepository;
            this.rentalsRepository = rentalsRepository;
            this.geographicalService = geographicalService;
        }
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

                return await this.ApplyFilters(filteredGamesArray, filter);
            }
            catch (Exception thrownException)
            {
                throw new InvalidOperationException("Failed to search for games.", thrownException);
            }
        }
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
        public bool IsValidPlayersCount(int? playersNumber)
        {
            if (!playersNumber.HasValue)
            {
                return true;
            }

            return playersNumber.Value >= MinimumAllowedPlayers;
        }
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
