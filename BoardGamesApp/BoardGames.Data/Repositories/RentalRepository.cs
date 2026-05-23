// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext context;

        public RentalRepository(AppDbContext appContext)
        {
            context = appContext;
        }

        // ==========================================
        // Project 1 methods (original)
        // ==========================================

        public async Task<Rental?> GetById(int rentalId)
        {
            return await context.Rentals
                .FirstOrDefaultAsync(rental => rental.Id == rentalId);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            return await context.Rentals
                .Where(rental => rental.Id == rentalId)
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

        // ==========================================
        // Project 2 methods (merged from RentalRepository2)
        // ==========================================

        private IQueryable<Rental> RentalsWithNavigations() =>
            context.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Client)
                .Include(rental => rental.Owner);

        public ImmutableList<Rental> GetAll()
        {
            return RentalsWithNavigations().ToImmutableList();
        }

        public void Add(Rental rental)
        {
            rental.Game = ResolveGame(rental.Game);
            rental.Client = ResolveUser(rental.Client);
            rental.Owner = ResolveUser(rental.Owner);
            context.Rentals.Add(rental);
            context.SaveChanges();

            var saved = RentalsWithNavigations().FirstOrDefault(savedRental => savedRental.Id == rental.Id);
            if (saved != null)
            {
                rental.Game = saved.Game;
                rental.Client = saved.Client;
                rental.Owner = saved.Owner;
            }
        }

        public void AddConfirmed(Rental rental) => Add(rental);

        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId)
        {
            return RentalsWithNavigations()
                .Where(rental => rental.Owner != null && rental.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId)
        {
            return RentalsWithNavigations()
                .Where(rental => rental.Client != null && rental.Client.Id == renterAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            return RentalsWithNavigations()
                .Where(rental => rental.Game != null && rental.Game.Id == gameId)
                .ToImmutableList();
        }

        public Rental Delete(int id)
        {
            var rental = RentalsWithNavigations().FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            context.Rentals.Remove(rental);
            context.SaveChanges();
            return rental;
        }

        public void Update(int id, Rental updated)
        {
            var existing = RentalsWithNavigations().FirstOrDefault(rental => rental.Id == id);
            if (existing == null)
            {
                return;
            }

            if (updated.Game != null)
            {
                existing.Game = ResolveGame(updated.Game);
            }

            if (updated.Client != null)
            {
                existing.Client = ResolveUser(updated.Client);
            }

            if (updated.Owner != null)
            {
                existing.Owner = ResolveUser(updated.Owner);
            }

            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            context.SaveChanges();
        }

        public Rental Get(int id)
        {
            var rental = RentalsWithNavigations().FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            return rental;
        }

        // ==========================================
        // Helpers
        // ==========================================

        private User? ResolveUser(User? user)
        {
            if (user == null)
            {
                return null;
            }

            return context.Users.Find(user.Id);
        }

        private Game? ResolveGame(Game? game)
        {
            if (game == null)
            {
                return null;
            }

            return context.Games.Find(game.Id);
        }
    }
}
