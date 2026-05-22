// <copyright file="IRentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace BoardGames.Data.Interfaces
{
    public interface IRentalRepository
    {
        Task<Rental?> GetById(int id);

        Task<TimeRange?> GetRentalTimeRange(int id);

        Task<List<TimeRange>> GetAllOccupiedPeriods();

        Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId);

        Task<bool> CheckGameAvailability(DateTime start, DateTime end, int gameId);

        Task AddRental(Rental rental);

        Task<List<Rental>> GetRentalsForUser(int userId);

        /// <summary>
        /// Reserved for the desktop client API proxy: creates a rental and a rental-request chat message for the owner.
        /// The server-side repository does not implement this; use <c>POST api/rentals/book</c>.
        /// </summary>
        Task BookGameWithRentalRequest(int clientId, int gameId, DateTime startDate, DateTime endDate);
    }
}
