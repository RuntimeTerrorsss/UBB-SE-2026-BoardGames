using BoardGames.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Enums;
using Moq;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class SearchAndFilterServiceTests
    {
        private readonly Mock<InterfaceGamesRepository> _mockGamesRepository;
        private readonly Mock<IUserRepository> _mockUsersRepository;
        private readonly Mock<IRentalRepository> _mockRentalsRepository;
        private readonly Mock<InterfaceGeographicalService> _mockGeographicalService;
        private readonly SearchAndFilterService _service;

        public SearchAndFilterServiceTests()
        {
            _mockGamesRepository = new Mock<InterfaceGamesRepository>();
            _mockUsersRepository = new Mock<IUserRepository>();
            _mockRentalsRepository = new Mock<IRentalRepository>();
            _mockGeographicalService = new Mock<InterfaceGeographicalService>();

            _service = new SearchAndFilterService(
                _mockGamesRepository.Object,
                _mockUsersRepository.Object,
                _mockRentalsRepository.Object,
                _mockGeographicalService.Object);
        }

        #region SearchGamesByFilter

        [Fact]
        public async Task SearchGamesByFilter_ValidFilter_ReturnsMappedGamesWithCachedOwners()
        {
            // Arrange
            var filter = new FilterCriteria { City = "TestCity" }; // Match the owner's city
            var gamesFromRepo = new List<Game>
            {
                new Game { Id = 1, OwnerId = 10, Name = "Game1", PricePerDay = 5m, MaximumPlayerNumber = 4, MinimumPlayerNumber = 2 },
                new Game { Id = 2, OwnerId = 10, Name = "Game2", PricePerDay = 10m, MaximumPlayerNumber = 6, MinimumPlayerNumber = 3 }
            };

            var owner = new User { Id = 10, City = "TestCity" };

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetGamesByFilter(It.IsAny<FilterCriteria>()))
                .ReturnsAsync(gamesFromRepo);
            _mockUsersRepository.Setup(mockUsersRepository => mockUsersRepository.GetGameById(10)).ReturnsAsync(owner);

            // Act
            var result = await _service.SearchGamesByFilter(filter);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal("TestCity", filter.City); // Verify filter city is properly restored
            Assert.Equal("TestCity", result[0].City);
            Assert.Equal("TestCity", result[1].City);

            _mockUsersRepository.Verify(mockUsersRepository => mockUsersRepository.GetGameById(10), Times.Once);
        }

        [Fact]
        public async Task SearchGamesByFilter_RepositoryThrows_ThrowsInvalidOperationException()
        {

            var filter = new FilterCriteria();
            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetGamesByFilter(It.IsAny<FilterCriteria>()))
                .ThrowsAsync(new Exception("DB Error"));


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SearchGamesByFilter(filter));
            Assert.Contains("Failed to search for games.", exception.Message);
        }

        #endregion

        #region GetGamesFeedAvailableTonightByUser

        [Fact]
        public async Task GetGamesFeedAvailableTonightByUser_ValidData_ReturnsMappedGames()
        {

            int userId = 1;
            var games = new List<Game> { new Game { Id = 1, OwnerId = 10, Name = "Game1" } };
            var owner = new User { Id = 10, City = "Cluj" };

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetGamesForFeedAvailableTonight(userId)).ReturnsAsync(games);
            _mockUsersRepository.Setup(mockUsersRepository => mockUsersRepository.GetGameById(10)).ReturnsAsync(owner);


            var result = await _service.GetGamesFeedAvailableTonightByUser(userId);


            Assert.Single(result);
            Assert.Equal("Game1", result[0].Name);
        }

        [Fact]
        public async Task GetGamesFeedAvailableTonightByUser_OwnerIsNull_SkipsGame()
        {

            int userId = 1;
            var games = new List<Game> { new Game { Id = 1, OwnerId = 10, Name = "Game1" } };

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetGamesForFeedAvailableTonight(userId)).ReturnsAsync(games);
            _mockUsersRepository.Setup(mockUsersRepository => mockUsersRepository.GetGameById(10)).ReturnsAsync((User)null);


            var result = await _service.GetGamesFeedAvailableTonightByUser(userId);


            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGamesFeedAvailableTonightByUser_RepositoryThrows_ThrowsInvalidOperationException()
        {

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetGamesForFeedAvailableTonight(It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB Error"));


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetGamesFeedAvailableTonightByUser(1));
            Assert.Contains("Failed to retrieve <<Available tonight>> feed.", exception.Message);
        }

        #endregion

        #region GetOtherGamesFeedByUser

        [Fact]
        public async Task GetOtherGamesFeedByUser_ValidData_ReturnsMappedGames()
        {

            int userId = 1;
            var games = new List<Game> { new Game { Id = 1, OwnerId = 10, Name = "Game1" } };
            var owner = new User { Id = 10, City = "Cluj" };

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetRemainingGamesForFeed(userId)).ReturnsAsync(games);
            _mockUsersRepository.Setup(mockUsersRepository => mockUsersRepository.GetGameById(10)).ReturnsAsync(owner);


            var result = await _service.GetOtherGamesFeedByUser(userId);


            Assert.Single(result);
        }

        [Fact]
        public async Task GetOtherGamesFeedByUser_OwnerIsNull_SkipsGame()
        {

            int userId = 1;
            var games = new List<Game> { new Game { Id = 1, OwnerId = 10, Name = "Game1" } };

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetRemainingGamesForFeed(userId)).ReturnsAsync(games);
            _mockUsersRepository.Setup(mockUsersRepository => mockUsersRepository.GetGameById(10)).ReturnsAsync((User)null);


            var result = await _service.GetOtherGamesFeedByUser(userId);


            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOtherGamesFeedByUser_RepositoryThrows_ThrowsInvalidOperationException()
        {

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetRemainingGamesForFeed(It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB Error"));


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetOtherGamesFeedByUser(1));
            Assert.Contains("Failed to retrieve <<Others>> feed.", exception.Message);
        }

        #endregion

        #region ApplyFilters

        [Fact]
        public async Task ApplyFilters_FilterByName_ReturnsMatchingGames()
        {

            var games = new[]
            {
                new GameDTO { Name = "Catan" },
                new GameDTO { Name = "Monopoly" }
            };
            var filter = new FilterCriteria { Name = "cat" };


            var result = await _service.ApplyFilters(games, filter);


            Assert.Single(result);
            Assert.Equal("Catan", result[0].Name);
        }

        [Fact]
        public async Task ApplyFilters_FilterByMaxPriceAndPlayerCount_ReturnsMatchingGames()
        {

            var games = new[]
            {
                new GameDTO { Name = "G1", Price = 10m, MaximumPlayerNumber = 4 },
                new GameDTO { Name = "G2", Price = 20m, MaximumPlayerNumber = 6 },
                new GameDTO { Name = "G3", Price = 15m, MaximumPlayerNumber = 2 }
            };
            var filter = new FilterCriteria { MaximumPrice = 15m, PlayerCount = 4 };


            var result = await _service.ApplyFilters(games, filter);


            Assert.Single(result);
            Assert.Equal("G1", result[0].Name);
        }

        [Fact]
        public async Task ApplyFilters_FilterByCity_AppliesNormalizationAndFiltering()
        {

            var games = new[]
            {
                new GameDTO { Name = "G1", City = "București" },
                new GameDTO { Name = "G2", City = "Cluj" }
            };
            var filter = new FilterCriteria { City = "Bucharest" };

            _mockGeographicalService.Setup(g => g.GetCityDetails("Bucharest"))
                .Returns((false, "Bucharest", 0, 0));


            var result = await _service.ApplyFilters(games, filter);


            Assert.Single(result);
            Assert.Equal("G1", result[0].Name);
        }

        [Theory]
        [InlineData(SortOption.PriceAscending, 10, 20)]
        [InlineData(SortOption.PriceDescending, 20, 10)]
        public async Task ApplyFilters_SortByPrice_ReturnsSortedGames(SortOption sortOption, decimal expectedFirst, decimal expectedSecond)
        {

            var games = new[]
            {
                new GameDTO { Name = "G1", Price = 20m },
                new GameDTO { Name = "G2", Price = 10m }
            };
            var filter = new FilterCriteria { SortOption = sortOption };


            var result = await _service.ApplyFilters(games, filter);


            Assert.Equal(2, result.Length);
            Assert.Equal(expectedFirst, result[0].Price);
            Assert.Equal(expectedSecond, result[1].Price);
        }

        [Fact]
        public async Task ApplyFilters_SortByLocation_OrdersByDistance()
        {

            var games = new[]
            {
                new GameDTO { Name = "FarGame", City = "Constanta" },
                new GameDTO { Name = "NearGame", City = "Cluj-Napoca" },
                new GameDTO { Name = "NoCityGame", City = null }
            };
            var filter = new FilterCriteria { City = "Cluj-Napoca", SortOption = SortOption.Location };

            _mockGeographicalService.Setup(g => g.GetCityDetails("Cluj-Napoca"))
                .Returns((true, "Cluj-Napoca", 46.77, 23.59));

            _mockGeographicalService.Setup(g => g.GetCityDetails("Constanta"))
                .Returns((true, "Constanta", 44.15, 28.63));


            var result = await _service.ApplyFilters(games, filter);


            Assert.Equal(3, result.Length);
            Assert.Equal("NearGame", result[0].Name);
            Assert.Equal("FarGame", result[1].Name);
            Assert.Equal("NoCityGame", result[2].Name);
        }

        [Fact]
        public async Task ApplyFilters_FilterByAvailability_ChecksRepository()
        {

            var games = new[]
            {
                new GameDTO { GameId = 1, Name = "Available" },
                new GameDTO { GameId = 2, Name = "Unavailable" }
            };
            var filter = new FilterCriteria { AvailabilityRange = new TimeRange(DateTime.Now, DateTime.Now.AddDays(1)) };

            _mockRentalsRepository.Setup(r => r.CheckGameAvailability(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1)).ReturnsAsync(true);
            _mockRentalsRepository.Setup(r => r.CheckGameAvailability(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 2)).ReturnsAsync(false);


            var result = await _service.ApplyFilters(games, filter);


            Assert.Single(result);
            Assert.Equal("Available", result[0].Name);
        }

        [Fact]
        public async Task ApplyFilters_ExceptionThrown_ThrowsInvalidOperationException()
        {

            var games = new[] { new GameDTO() };
            var filter = new FilterCriteria { AvailabilityRange = new TimeRange(DateTime.Now, DateTime.Now) };

            _mockRentalsRepository.Setup(r => r.CheckGameAvailability(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB Fault"));


            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ApplyFilters(games, filter));
        }

        #endregion

        #region GetDiscoveryFeedPaged

        [Fact]
        public async Task GetDiscoveryFeedPaged_ReturnsCorrectlyPaginatedAndSeparatedLists()
        {

            int userId = 1;


            var availableGames = new List<Game>
            {
                new Game { Id = 1, OwnerId = 10, Name = "Tonight1" },
                new Game { Id = 2, OwnerId = 10, Name = "Tonight2" }
            };


            var otherGames = new List<Game>
            {
                new Game { Id = 3, OwnerId = 10, Name = "Other1" },
                new Game { Id = 4, OwnerId = 10, Name = "Other2" }
            };

            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetGamesForFeedAvailableTonight(userId)).ReturnsAsync(availableGames);
            _mockGamesRepository.Setup(mockGamesRepository => mockGamesRepository.GetRemainingGamesForFeed(userId)).ReturnsAsync(otherGames);
            _mockUsersRepository.Setup(mockUsersRepository => mockUsersRepository.GetGameById(10)).ReturnsAsync(new User { Id = 10 });


            var (tonight, others, total) = await _service.GetDiscoveryFeedPaged(userId, page: 2, pageSize: 2);


            Assert.Equal(4, total);


            Assert.Empty(tonight);
            Assert.Equal(2, others.Count);
            Assert.Equal(3, others[0].GameId);
            Assert.Equal(4, others[1].GameId);
        }

        #endregion

        #region IsValidDateRange & IsValidPlayersCount

        [Theory]
        [InlineData(null, null, true)]
        [InlineData("2025-01-01", null, false)]
        [InlineData(null, "2025-01-01", false)]
        [InlineData("2025-01-01", "2025-01-02", true)]
        [InlineData("2025-01-01", "2025-01-01", true)]
        [InlineData("2025-01-02", "2025-01-01", false)]
        public void IsValidDateRange_EvaluatesCorrectly(string? startStr, string? endStr, bool expected)
        {

            DateTime? start = startStr != null ? DateTime.Parse(startStr) : null;
            DateTime? end = endStr != null ? DateTime.Parse(endStr) : null;


            bool result = _service.IsValidDateRange(start, end);


            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(-1, false)]
        public void IsValidPlayersCount_EvaluatesCorrectly(int? count, bool expected)
        {

            bool result = _service.IsValidPlayersCount(count);


            Assert.Equal(expected, result);
        }

        #endregion

        #region UpdateFilterFromUI

        [Fact]
        public void UpdateFilterFromUI_BothDatesNull_SetsAvailabilityRangeToNull()
        {


            var filter = new FilterCriteria
            {
                AvailabilityRange = new TimeRange(DateTime.Now, DateTime.Now.AddDays(1))
            };




            _service.UpdateFilterFromUI(filter, 50.0, 4.0, null, null);


            Assert.Null(filter.AvailabilityRange);
        }

        [Fact]
        public void UpdateFilterFromUI_UpdatesFieldsProperly_WhenValid()
        {

            var filter = new FilterCriteria();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(1);


            _service.UpdateFilterFromUI(filter, 50.0, 4.0, startDate, endDate);


            Assert.Equal(50m, filter.MaximumPrice);
            Assert.Equal(4, filter.PlayerCount);
            Assert.NotNull(filter.AvailabilityRange);
            Assert.Equal(startDate, filter.AvailabilityRange.StartTime);
            Assert.Equal(endDate, filter.AvailabilityRange.EndTime);
        }

        [Fact]
        public void UpdateFilterFromUI_AssignsNull_WhenValuesAreZeroOrInvalid()
        {

            var filter = new FilterCriteria { MaximumPrice = 100m, PlayerCount = 4, AvailabilityRange = new TimeRange(DateTime.Now, DateTime.Now) };


            _service.UpdateFilterFromUI(filter, 0.0, 0.0, DateTime.Now, DateTime.Now.AddDays(-1));


            Assert.Null(filter.MaximumPrice);
            Assert.Null(filter.PlayerCount);
            Assert.Null(filter.AvailabilityRange);
        }

        #endregion

        #region Private Method Mapping Tests

        [Fact]
        public void MapToGameDTO_OwnerIsNull_FallsBackToEmptyCity()
        {


            var methodInfo = typeof(SearchAndFilterService).GetMethod(
                "MapToGameDTO",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var gameEntity = new Game { Id = 1, Name = "Catan", PricePerDay = 10, MaximumPlayerNumber = 4, MinimumPlayerNumber = 2 };
            User? nullOwner = null;


            var result = (GameDTO)methodInfo.Invoke(_service, new object[] { gameEntity, nullOwner });


            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.City);
            Assert.Equal(gameEntity.Id, result.GameId);
        }

        [Fact]
        public void MapToGameDTO_OwnerCityIsNull_FallsBackToEmptyString()
        {


            var methodInfo = typeof(SearchAndFilterService).GetMethod(
                "MapToGameDTO",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var gameEntity = new Game { Id = 2, Name = "Monopoly", PricePerDay = 15, MaximumPlayerNumber = 6, MinimumPlayerNumber = 2 };
            var ownerWithNullCity = new User { Id = 10, City = null };


            var result = (GameDTO)methodInfo.Invoke(_service, new object[] { gameEntity, ownerWithNullCity });


            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.City);
        }

        #endregion
    }
}