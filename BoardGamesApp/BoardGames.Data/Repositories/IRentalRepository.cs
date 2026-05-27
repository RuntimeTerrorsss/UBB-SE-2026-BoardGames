// <copyright file="IRentalRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IRentalRepository
    {
        // --- Project 1 methods ---
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

        // --- Project 2 methods (merged from IRentalRepository2) ---
        ImmutableList<Rental> GetAll();

        void Add(Rental rental);

        Rental Delete(int id);

        void Update(int id, Rental updated);

        Rental Get(int id);

        void AddConfirmed(Rental confirmedRental);

        ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId);

        ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId);

        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}
