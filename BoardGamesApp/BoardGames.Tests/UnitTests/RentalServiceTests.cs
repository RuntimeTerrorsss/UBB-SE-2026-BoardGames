using Moq;
using Xunit;
// <copyright file="RentalServiceTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Data.Repositories;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Tests.UnitTests
{
    public class RentalServiceTests
    {
        private readonly Mock<IRentalRepository> _mockRentalRepository;
        private readonly Mock<InterfaceGamesRepository> _mockGameRepository;
        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            this._mockRentalRepository = new Mock<IRentalRepository>();
            this._mockGameRepository = new Mock<InterfaceGamesRepository>();

            this._rentalService = new RentalService(
                this._mockRentalRepository.Object,
                this._mockGameRepository.Object);
        }

        #region GetRentalById

        [Fact]
        public async Task GetRentalById_ValidId_ReturnsRentalFromRepository()
        {
            int rentalId = 1;
            var expectedRental = new Rental { GameId = 2 };
            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetById(rentalId)).ReturnsAsync(expectedRental);

            var result = await this._rentalService.GetRentalById(rentalId);

            Assert.Equal(expectedRental, result);
            this._mockRentalRepository.Verify(mockRentalRepository => mockRentalRepository.GetById(rentalId), Times.Once);
        }

        #endregion

        #region GetRentalPrice

        [Fact]
        public async Task GetRentalPrice_RentalNotFound_ReturnsZero()
        {
            int rentalId = 1;
            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetById(rentalId)).ReturnsAsync((Rental)null);

            var result = await this._rentalService.GetRentalPrice(rentalId);

            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetRentalPrice_RentalFound_CalculatesAndReturnsCorrectPrice()
        {
            int rentalId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(2);
            var rental = new Rental { GameId = 5, StartDate = startDate, EndDate = endDate };
            decimal pricePerDay = 15m;
            decimal expectedTotalPrice = 45m;

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetById(rentalId)).ReturnsAsync(rental);
            this._mockGameRepository.Setup(mockGameRepository => mockGameRepository.GetPriceGameById(rental.GameId)).ReturnsAsync(pricePerDay);

            var result = await this._rentalService.GetRentalPrice(rentalId);

            Assert.Equal(expectedTotalPrice, result);
        }

        #endregion

        #region GetGameName

        [Fact]
        public async Task GetGameName_RentalNotFound_ReturnsUnknownRental()
        {
            int rentalId = 1;
            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetById(rentalId)).ReturnsAsync((Rental)null);

            var result = await this._rentalService.GetGameName(rentalId);

            Assert.Equal("Unknown Rental", result);
        }

        [Fact]
        public async Task GetGameName_GameNotFound_ReturnsUnknownGame()
        {
            int rentalId = 1;
            var rental = new Rental { GameId = 5 };

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetById(rentalId)).ReturnsAsync(rental);
            this._mockGameRepository.Setup(g => g.GetGameById(rental.GameId)).ReturnsAsync((Game)null);

            var result = await this._rentalService.GetGameName(rentalId);

            Assert.Equal("Unknown Game", result);
        }

        [Fact]
        public async Task GetGameName_ValidRentalAndGame_ReturnsGameName()
        {
            int rentalId = 1;
            var rental = new Rental { GameId = 5 };
            var game = new Game { Name = "Catan" };

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetById(rentalId)).ReturnsAsync(rental);
            this._mockGameRepository.Setup(mockGameRepository => mockGameRepository.GetGameById(rental.GameId)).ReturnsAsync(game);

            var result = await this._rentalService.GetGameName(rentalId);

            Assert.Equal("Catan", result);
        }

        #endregion

        #region GetUnavailableTimeRanges

        [Fact]
        public async Task GetUnavailableTimeRanges_ValidGameId_ReturnsRangesFromRepository()
        {
            int gameId = 1;
            var expectedRanges = new List<TimeRange>
            {
                new TimeRange(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)),
            };

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.GetUnavailableTimeRanges(gameId)).ReturnsAsync(expectedRanges);

            var result = await this._rentalService.GetUnavailableTimeRanges(gameId);

            Assert.Equal(expectedRanges, result);
        }

        #endregion

        #region CheckGameAvailability

        [Fact]
        public async Task CheckGameAvailability_EndDateBeforeStartDate_ReturnsFalse()
        {
            int gameId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-1);

            var result = await this._rentalService.CheckGameAvailability(gameId, startDate, endDate);

            Assert.False(result);
            this._mockRentalRepository.Verify(mockRentalRepository => mockRentalRepository.CheckGameAvailability(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckGameAvailability_ValidDates_ReturnsRepositoryResult(bool repositoryResult)
        {
            int gameId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(1);

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.CheckGameAvailability(startDate, endDate, gameId)).ReturnsAsync(repositoryResult);

            var result = await this._rentalService.CheckGameAvailability(gameId, startDate, endDate);

            Assert.Equal(repositoryResult, result);
        }

        #endregion

        #region CalculateTotalPriceForRentingASpecificGame

        [Fact]
        public async Task CalculateTotalPriceForRentingASpecificGame_ValidInput_ReturnsCorrectTotal()
        {
            decimal price = 20m;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(2);
            var timeRange = new TimeRange(startDate, endDate);

            decimal expectedTotal = 60m;

            var result = await this._rentalService.CalculateTotalPriceForRentingASpecificGame(price, timeRange);

            Assert.Equal(expectedTotal, result);
        }

        #endregion

        #region CalculateNumberOfDaysInAGivenTimeRange

        [Fact]
        public async Task CalculateNumberOfDaysInAGivenTimeRange_PositiveDifference_ReturnsActualDaysPlusOne()
        {
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(4);
            var timeRange = new TimeRange(startDate, endDate);

            var result = await this._rentalService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            Assert.Equal(5, result);
        }

        [Fact]
        public async Task CalculateNumberOfDaysInAGivenTimeRange_ZeroDifference_ReturnsMinimumValidDayCount()
        {
            var sameDate = DateTime.UtcNow;
            var timeRange = new TimeRange(sameDate, sameDate);

            var result = await this._rentalService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task CalculateNumberOfDaysInAGivenTimeRange_SameStartAndEndDate_ReturnsMinimumValidDayCount()
        {
            var startDate = DateTime.UtcNow;
            var endDate = startDate;
            var timeRange = new TimeRange(startDate, endDate);

            var result = await this._rentalService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            Assert.Equal(1, result);
        }

        #endregion

        #region CreateRental

        [Fact]
        public async Task CreateRental_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            int gameId = 1, clientId = 2, ownerId = 3;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-1);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => this._rentalService.CreateRental(gameId, clientId, ownerId, startDate, endDate));

            Assert.Equal("End date must be after start date.", exception.Message);
        }

        [Fact]
        public async Task CreateRental_GameUnavailable_ThrowsInvalidOperationException()
        {
            int gameId = 1, clientId = 2, ownerId = 3;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(1);

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.CheckGameAvailability(startDate, endDate, gameId)).ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => this._rentalService.CreateRental(gameId, clientId, ownerId, startDate, endDate));

            Assert.Equal("The game is not available for the selected period.", exception.Message);
        }

        [Fact]
        public async Task CreateRental_ValidRequest_CreatesCalculatesPriceAndReturnsRental()
        {
            int gameId = 1, clientId = 2, ownerId = 3;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(2);
            decimal pricePerDay = 10m;
            decimal expectedTotalPrice = 30m;

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.CheckGameAvailability(startDate, endDate, gameId)).ReturnsAsync(true);
            this._mockGameRepository.Setup(mockGameRepository => mockGameRepository.GetPriceGameById(gameId)).ReturnsAsync(pricePerDay);

            this._mockRentalRepository.Setup(mockRentalRepository => mockRentalRepository.AddRental(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            var result = await this._rentalService.CreateRental(gameId, clientId, ownerId, startDate, endDate);

            Assert.NotNull(result);
            Assert.Equal(gameId, result.GameId);
            Assert.Equal(clientId, result.ClientId);
            Assert.Equal(ownerId, result.OwnerId);
            Assert.Equal(startDate, result.StartDate);
            Assert.Equal(endDate, result.EndDate);
            Assert.Equal(expectedTotalPrice, result.TotalPrice);

            this._mockRentalRepository.Verify(mockRentalRepository => mockRentalRepository.AddRental(It.Is<Rental>(ren => ren.TotalPrice == expectedTotalPrice)), Times.Once);
        }

        #endregion
    }
}
