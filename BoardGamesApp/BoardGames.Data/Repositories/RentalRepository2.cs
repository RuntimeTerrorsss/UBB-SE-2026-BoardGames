/*
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class RentalRepository2 : IRentalRepository2
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public RentalRepository2(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Rental> RentalsWithNavigations(AppDbContext dbContext) =>
            dbContext.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Client)
                .Include(rental => rental.Owner);

        public ImmutableList<Rental> GetAll()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext).ToImmutableList();
        }

        public void Add(Rental rental)
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            rental.Game = ResolveGame(dbContext, rental.Game);
            rental.Client = ResolveUser(dbContext, rental.Client);
            rental.Owner = ResolveUser(dbContext, rental.Owner);
            dbContext.Rentals.Add(rental);
            dbContext.SaveChanges();

            var saved = RentalsWithNavigations(dbContext).FirstOrDefault(savedRental => savedRental.Id == rental.Id);
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
            using var dbContext = dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext)
                .Where(rental => rental.Owner != null && rental.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext)
                .Where(rental => rental.Client != null && rental.Client.Id == renterAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext)
                .Where(rental => rental.Game != null && rental.Game.Id == gameId)
                .ToImmutableList();
        }

        public Rental Delete(int id)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var rental = RentalsWithNavigations(dbContext).FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Rentals.Remove(rental);
            dbContext.SaveChanges();
            return rental;
        }

        public void Update(int id, Rental updated)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var existing = RentalsWithNavigations(dbContext).FirstOrDefault(rental => rental.Id == id);
            if (existing == null)
            {
                return;
            }

            if (updated.Game != null)
            {
                existing.Game = ResolveGame(dbContext, updated.Game);
            }

            if (updated.Client != null)
            {
                existing.Client = ResolveUser(dbContext, updated.Client);
            }

            if (updated.Owner != null)
            {
                existing.Owner = ResolveUser(dbContext, updated.Owner);
            }

            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            dbContext.SaveChanges();
        }

        public Rental Get(int id)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var rental = RentalsWithNavigations(dbContext).FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            return rental;
        }

        private static User? ResolveUser(AppDbContext dbContext, User? user)
        {
            if (user == null)
            {
                return null;
            }

            return dbContext.Users.Find(user.Id);
        }

        private static Game? ResolveGame(AppDbContext dbContext, Game? game)
        {
            if (game == null)
            {
                return null;
            }

            return dbContext.Games.Find(game.Id);
        }
    }
}
*/
