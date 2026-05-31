// <copyright file="RentalRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext context;

        public RentalRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public async Task<Rental?> GetById(int rentalId)
        {
            return await this.context.Rentals
                .FirstOrDefaultAsync(rental => rental.Id == rentalId);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            return await this.context.Rentals
                .Where(rental => rental.Id == rentalId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .FirstOrDefaultAsync();
        }

        public async Task<List<TimeRange>> GetAllOccupiedPeriods()
        {
            return await this.context.Rentals
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToListAsync();
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await this.context.Rentals
                .Where(rental => rental.GameId == gameId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToListAsync();
        }

        public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            var requestStart = startTime.Date;
            var requestEnd = endTime.Date;

            bool hasOverlap = await this.context.Rentals.AnyAsync(rental =>
                rental.GameId == gameId &&
                rental.StartDate.Date <= requestEnd &&
                rental.EndDate.Date >= requestStart);
            return !hasOverlap;
        }

        public async Task AddRental(Rental rental)
        {
            await this.context.Rentals.AddAsync(rental);
            await this.context.SaveChangesAsync();
        }

        public async Task<List<Rental>> GetRentalsForUser(int userId)
        {
            return await this.context.Rentals
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

        private IQueryable<Rental> RentalsWithNavigations() =>
            this.context.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Client)
                .Include(rental => rental.Owner);

        public ImmutableList<Rental> GetAll()
        {
            return this.RentalsWithNavigations().ToImmutableList();
        }

        public void Add(Rental rental)
        {
            rental.Game = this.ResolveGame(rental.Game);
            rental.Client = this.ResolveUser(rental.Client);
            rental.Owner = this.ResolveUser(rental.Owner);
            this.context.Rentals.Add(rental);
            this.context.SaveChanges();

            var saved = this.RentalsWithNavigations().FirstOrDefault(savedRental => savedRental.Id == rental.Id);
            if (saved != null)
            {
                rental.Game = saved.Game;
                rental.Client = saved.Client;
                rental.Owner = saved.Owner;
            }
        }

        public void AddConfirmed(Rental rental) => this.Add(rental);

        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId)
        {
            return this.RentalsWithNavigations()
                .Where(rental => rental.Owner != null && rental.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId)
        {
            return this.RentalsWithNavigations()
                .Where(rental => rental.Client != null && rental.Client.Id == renterAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            return this.RentalsWithNavigations()
                .Where(rental => rental.GameId == gameId)
                .ToImmutableList();
        }

        public Rental Delete(int id)
        {
            var rental = this.RentalsWithNavigations().FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            this.context.Rentals.Remove(rental);
            this.context.SaveChanges();
            return rental;
        }

        public void Update(int id, Rental updated)
        {
            var existing = this.RentalsWithNavigations().FirstOrDefault(rental => rental.Id == id);
            if (existing == null)
            {
                return;
            }

            if (updated.Game != null)
            {
                existing.Game = this.ResolveGame(updated.Game);
            }

            if (updated.Client != null)
            {
                existing.Client = this.ResolveUser(updated.Client);
            }

            if (updated.Owner != null)
            {
                existing.Owner = this.ResolveUser(updated.Owner);
            }

            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            this.context.SaveChanges();
        }

        public Rental Get(int id)
        {
            var rental = this.RentalsWithNavigations().FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            return rental;
        }

        private User? ResolveUser(User? user)
        {
            if (user == null)
            {
                return null;
            }

            return this.context.Users.Find(user.Id);
        }

        private Game? ResolveGame(Game? game)
        {
            if (game == null)
            {
                return null;
            }

            return this.context.Games.Find(game.Id);
        }
    }
}
