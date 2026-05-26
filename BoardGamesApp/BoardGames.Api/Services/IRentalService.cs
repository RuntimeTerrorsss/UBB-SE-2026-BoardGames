// <copyright file="IRentalService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IRentalService
    {
        public Task<List<RentalDTO>> GetRentalsForUser(int userId);

        public Task<Rental> GetRentalById(int rentalId);

        public Task<decimal> GetRentalPrice(int rentalId);

        public Task<string> GetGameName(int rentalId);

        public Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId);

        public Task<bool> CheckGameAvailability(int gameId, DateTime startDate, DateTime endDate);

        public Task<decimal> CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange);

        public Task<int> CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange);

        public Task<Rental> CreateRental(int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate);
    }
}
