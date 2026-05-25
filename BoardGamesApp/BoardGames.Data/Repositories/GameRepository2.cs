using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public GameRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Game> GamesWithOwner(AppDbContext dbContext) =>
            dbContext.Games.Include(game => game.Owner);

        public ImmutableList<Game> GetAll()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return GamesWithOwner(dbContext).ToImmutableList();
        }

        public void Add(Game game)
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            if (game.Owner != null)
            {
                var owner = ResolveUser(dbContext, game.Owner);
                game.Owner = owner;
                game.OwnerId = owner.PamUserId;
            }

            dbContext.Games.Add(game);
            dbContext.SaveChanges();
        }

        public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return GamesWithOwner(dbContext)
                .Where(game => game.Owner != null && game.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public void Update(int id, Game updated)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var existing = GamesWithOwner(dbContext).FirstOrDefault(game => game.Id == id);
            if (existing == null)
            {
                throw new KeyNotFoundException();
            }

            if (updated.Owner != null)
            {
                existing.Owner = ResolveUser(dbContext, updated.Owner);
            }

            existing.Name = updated.Name;
            existing.PricePerDay = updated.PricePerDay;
            existing.MinimumPlayerNumber = updated.MinimumPlayerNumber;
            existing.MaximumPlayerNumber = updated.MaximumPlayerNumber;
            existing.Description = updated.Description;
            existing.Image = updated.Image;
            existing.IsActive = updated.IsActive;

            dbContext.SaveChanges();
        }

        public Game Get(int id)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var game = GamesWithOwner(dbContext).FirstOrDefault(repositoryGame => repositoryGame.Id == id);
            if (game == null)
            {
                throw new KeyNotFoundException();
            }

            return game;
        }

        public Game Delete(int id)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var game = GamesWithOwner(dbContext).FirstOrDefault(repositoryGame => repositoryGame.Id == id);
            if (game == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Games.Remove(game);
            dbContext.SaveChanges();
            return game;
        }

        private User ResolveUser(AppDbContext dbContext, User user)
        {
            if (user == null) return null!;

            if (user.PamUserId != 0)
            {
                var tracked = dbContext.Users.Local.FirstOrDefault(u => u.PamUserId == user.PamUserId)
                             ?? dbContext.Users.SingleOrDefault(u => u.PamUserId == user.PamUserId);
                if (tracked != null) return tracked;
                throw new InvalidOperationException($"User with PamUserId {user.PamUserId} was not found.");
            }

            if (user.Id != Guid.Empty)
            {
                var tracked = dbContext.Users.Local.FirstOrDefault(u => u.Id == user.Id)
                             ?? dbContext.Users.SingleOrDefault(u => u.Id == user.Id);
                if (tracked != null) return tracked;
                throw new InvalidOperationException($"User with Id {user.Id} was not found.");
            }

            throw new InvalidOperationException("Owner must include a valid PamUserId or Id.");
        }
    }
}
