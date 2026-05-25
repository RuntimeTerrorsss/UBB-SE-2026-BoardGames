// <copyright file="IRentalService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IRentalService
    {
        public Task<List<RentalDataTransferObject>> GetRentalsForUser(int userId);

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
