// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGames.Data.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext context;

        public RentalRepository(AppDbContext appContext)
        {
            context = appContext;
        }

        public async Task<Rental?> GetById(int rentalId)
        {
            return await context.Rentals
                .FirstOrDefaultAsync(rental => rental.RentalId == rentalId);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            return await context.Rentals
                .Where(rental => rental.RentalId == rentalId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .FirstOrDefaultAsync();
        }

        public async Task<List<TimeRange>> GetAllOccupiedPeriods()
        {
            return await context.Rentals
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToListAsync();
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await context.Rentals
                .Where(rental => rental.GameId == gameId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToListAsync();
        }

        public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            var requestStart = startTime.Date;
            var requestEnd = endTime.Date;

            bool hasOverlap = await context.Rentals.AnyAsync(rental =>
                rental.GameId == gameId &&
                rental.StartDate.Date <= requestEnd &&
                rental.EndDate.Date >= requestStart);
            return !hasOverlap;
        }

        public async Task AddRental(Rental rental)
        {
            await context.Rentals.AddAsync(rental);
            await context.SaveChangesAsync();
        }

        public async Task<List<Rental>> GetRentalsForUser(int userId)
        {
            return await context.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Client)
                .Include(rental => rental.Owner)
                .Where(rental => rental.ClientId == userId || rental.OwnerId == userId)
                .OrderByDescending(rental => rental.StartDate)
                .ToListAsync();
        }

        public Task BookGameWithRentalRequest(int clientId, int gameId, DateTime startDate, DateTime endDate)
        {
            throw new NotSupportedException("Use POST api/rentals/book; this repository only persists rentals.");
        }
    }
}
