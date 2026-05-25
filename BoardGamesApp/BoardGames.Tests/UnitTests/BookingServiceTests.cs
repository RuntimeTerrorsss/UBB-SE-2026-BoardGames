using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Api.Services;
using Moq;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class BookingServiceTests
    {
        private readonly Mock<InterfaceGamesRepository> _mockGamesRepository;
        private readonly Mock<IRentalRepository> _mockRentalsRepository;
        private readonly Mock<IUserRepository> _mockUsersRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockGamesRepository = new Mock<InterfaceGamesRepository>();
            _mockRentalsRepository = new Mock<IRentalRepository>();
            _mockUsersRepository = new Mock<IUserRepository>();

            _bookingService = new BookingService(
                _mockGamesRepository.Object,
                _mockRentalsRepository.Object,
                _mockUsersRepository.Object);
        }

        #region GetBookingInformationForSpecificGame

        [Fact]
        public async Task GetBookingInformationForSpecificGame_GameIsNull_ThrowsInvalidOperationException()
        {

            var gameId = 1;
            _mockGamesRepository.Setup(gameRepo => gameRepo.GetGameById(gameId)).ReturnsAsync((Game)null);


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.GetBookingInformationForSpecificGame(gameId));

            Assert.Equal($"Game with id {gameId} was not found.", exception.Message);
        }

        [Fact]
        public async Task GetBookingInformationForSpecificGame_OwnerIsNull_ThrowsInvalidOperationException()
        {

            var gameId = 1;
            var ownerId = 2;
            var mockGame = new Game { Id = gameId, Name = "Test Game", OwnerId = ownerId };

            _mockGamesRepository.Setup(gameRepo => gameRepo.GetGameById(gameId)).ReturnsAsync(mockGame);
            _mockUsersRepository.Setup(userRepo => userRepo.GetGameById(ownerId)).ReturnsAsync((User)null);


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.GetBookingInformationForSpecificGame(gameId));

            Assert.Equal($"Owner for game id {gameId} was not found.", exception.Message);
        }

        [Fact]
        public void CalculateTotalPriceForRentingASpecificGame_NegativeDays_HitsMinimumDayCountBranch()
        {

            var pricePerDay = 15m;
            var startTime = DateTime.UtcNow;





            var endTime = startTime.AddDays(-2);

            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };


            var expectedPrice = 1 * pricePerDay;


            var result = _bookingService.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);


            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public async Task GetBookingInformationForSpecificGame_ValidData_ReturnsBookingDTO()
        {

            var gameId = 1;
            var ownerId = 2;
            var mockGame = new Game
            {
                Id = gameId,
                Name = "Test",
                Image = Array.Empty<byte>(),
                PricePerDay = 10m,
                OwnerId = ownerId,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = "Desc"
            };
            var mockOwner = new User
            {
                Id = ownerId,
                DisplayName = "John Doe",
                City = "New York",
                IsSuspended = false,
                AvatarUrl = "avatar.png",
                CreatedAt = DateTime.UtcNow
            };

            _mockGamesRepository.Setup(gameRepo => gameRepo.GetGameById(gameId)).ReturnsAsync(mockGame);
            _mockUsersRepository.Setup(userRepo => userRepo.GetGameById(ownerId)).ReturnsAsync(mockOwner);


            var result = await _bookingService.GetBookingInformationForSpecificGame(gameId);


            Assert.NotNull(result);
            Assert.Equal(mockGame.Id, result.GameId);
            Assert.Equal(mockGame.Name, result.Name);
            Assert.Equal(mockOwner.DisplayName, result.DisplayName);
            Assert.Equal(mockOwner.City, result.City);
        }

        [Fact]
        public async Task GetBookingInformationForSpecificGame_RepositoryThrows_RethrowsException()
        {

            var gameId = 1;
            var expectedException = new Exception("Database failure");
            _mockGamesRepository.Setup(gameRepo => gameRepo.GetGameById(gameId)).ThrowsAsync(expectedException);


            var exception = await Assert.ThrowsAsync<Exception>(
                () => _bookingService.GetBookingInformationForSpecificGame(gameId));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        #endregion

        #region GetUnavailableTimeRanges

        [Fact]
        public async Task GetUnavailableTimeRanges_ValidRequest_ReturnsTimeRangeArray()
        {

            var gameId = 1;
            var mockRanges = new List<TimeRange>
            {
                new TimeRange { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(1) }
            };

            _mockRentalsRepository.Setup(rentalRepo => rentalRepo.GetUnavailableTimeRanges(gameId))
                                  .ReturnsAsync(mockRanges);


            var result = await _bookingService.GetUnavailableTimeRanges(gameId);


            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetUnavailableTimeRanges_RepositoryThrows_ThrowsInvalidOperationException()
        {

            var gameId = 1;
            _mockRentalsRepository.Setup(rentalRepo => rentalRepo.GetUnavailableTimeRanges(gameId))
                                  .ThrowsAsync(new Exception("DB Error"));


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.GetUnavailableTimeRanges(gameId));

            Assert.StartsWith($"Failed to retrieve unavailable time ranges for game {gameId}.", exception.Message);
        }

        #endregion

        #region CheckGameAvailability

        [Fact]
        public async Task CheckGameAvailability_AvailableGame_ReturnsTrue()
        {

            var gameId = 1;
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow.AddDays(1);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(rentalRepo => rentalRepo.CheckGameAvailability(startTime, endTime, gameId))
                                  .ReturnsAsync(true);


            var result = await _bookingService.CheckGameAvailability(gameId, timeRange);


            Assert.True(result);
        }

        [Fact]
        public async Task CheckGameAvailability_RepositoryThrows_ThrowsInvalidOperationException()
        {

            var gameId = 1;
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow.AddDays(1);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(rentalRepo => rentalRepo.CheckGameAvailability(startTime, endTime, gameId))
                                  .ThrowsAsync(new Exception("DB Error"));


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.CheckGameAvailability(gameId, timeRange));

            Assert.StartsWith($"Failed to check availability for game {gameId}.", exception.Message);
        }

        #endregion

        #region CalculateTotalPriceForRentingASpecificGame

        [Fact]
        public void CalculateTotalPriceForRentingASpecificGame_MultipleDays_ReturnsCorrectPrice()
        {

            var pricePerDay = 10m;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(3);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };


            var expectedPrice = 4 * pricePerDay;


            var result = _bookingService.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);


            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public void CalculateTotalPriceForRentingASpecificGame_NegativeOrZeroDays_AppliesMinimumDayCount()
        {

            var pricePerDay = 15m;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddHours(-5);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };


            var expectedPrice = 1 * pricePerDay;


            var result = _bookingService.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);


            Assert.Equal(expectedPrice, result);
        }

        #endregion

        #region CalculateNumberOfDaysInAGivenTimeRange

        [Fact]
        public void CalculateNumberOfDaysInAGivenTimeRange_ValidDifference_ReturnsCalculatedDays()
        {

            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };


            var result = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);


            Assert.Equal(3, result);
        }

        [Fact]
        public void CalculateNumberOfDaysInAGivenTimeRange_ZeroDifference_ReturnsMinimumValidDayCount()
        {

            var time = DateTime.UtcNow;
            var timeRange = new TimeRange { StartTime = time, EndTime = time };


            var result = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);


            Assert.Equal(1, result);
        }

        [Fact]
        public void CalculateNumberOfDaysInAGivenTimeRange_NegativeDifference_ReturnsMinimumValidDayCount()
        {

            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(-2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };


            var result = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);


            Assert.Equal(1, result);
        }

        #endregion

        #region AddBooking

        [Fact]
        public async Task AddBooking_ClientIdIsZeroOrLess_ThrowsInvalidOperationException()
        {

            var gameId = 1;
            var clientId = 0;
            var timeRange = new TimeRange { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(1) };


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.AddBooking(gameId, clientId, timeRange));

            Assert.Equal("A valid logged-in renter account is required to complete a booking.", exception.Message);
        }

        [Fact]
        public async Task AddBooking_ValidRequest_CallsBookGameWithRentalRequest()
        {

            var gameId = 1;
            var clientId = 2;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(rentalRepo => rentalRepo.BookGameWithRentalRequest(clientId, gameId, startTime, endTime))
                                  .Returns(Task.CompletedTask);


            await _bookingService.AddBooking(gameId, clientId, timeRange);


            _mockRentalsRepository.Verify(rentalRepo => rentalRepo.BookGameWithRentalRequest(clientId, gameId, startTime, endTime), Times.Once);
        }

        [Fact]
        public async Task AddBooking_RepositoryThrows_ThrowsInvalidOperationException()
        {

            var gameId = 1;
            var clientId = 2;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(rentalRepo => rentalRepo.BookGameWithRentalRequest(clientId, gameId, startTime, endTime))
                                  .ThrowsAsync(new Exception("DB Connection Failed"));


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.AddBooking(gameId, clientId, timeRange));

            Assert.StartsWith($"Failed to add booking for game {gameId}.", exception.Message);
        }

        #endregion
    }
}