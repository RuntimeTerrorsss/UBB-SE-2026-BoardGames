// <copyright file="GamesRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories;

public class GamesRepository : InterfaceGamesRepository, IGameRepository
{
    /// <summary>
    /// Represents the ID used for unauthenticated users.
    /// </summary>
    public const int AnonymousUserId = -1;

    private readonly AppDbContext appContext;

    public GamesRepository(AppDbContext context)
    {
        this.appContext = context;
    }

    /// <summary>
    /// Gets a single game by its database id.
    /// </summary>
    /// <param name="gameId">The unique id of the game.</param>
    /// <returns>The game object if found; otherwise, null.</returns>
    public async Task<Game?> GetGameById(int gameId)
    {
        return await this.appContext.Games.FirstOrDefaultAsync(game => game.Id == gameId);
    }

    public async Task<decimal> GetPriceGameById(int gameId)
    {
        return await this.appContext.Games.Where(game => game.Id == gameId).Select(game => game.PricePerDay).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all active games that are visible in the system.
    /// </summary>
    /// <returns>A list of all active games.</returns>
    public async Task<List<Game>> GetAll()
    {
        return await this.GetAllActiveGames(AnonymousUserId);
    }

    /// <summary>
    /// Gets all games including inactive ones (for Admin).
    /// </summary>
    /// <returns>A list of all games.</returns>
    public async Task<List<Game>> GetAllIncludingInactive()
    {
        return await appContext.Games.Include(game => game.Owner).ToListAsync();
    }

    /// <summary>
    /// Gets games that match the provided filter criteria.
    /// </summary>
    /// <param name="filter">
    /// Object containing user-entered search/filter values.
    /// All fields may be empty/null.
    /// </param>
    /// <returns>A list of games matching the filter.</returns>
    public async Task<List<Game>> GetGamesByFilter(FilterCriteria filter)
    {
        var userId = filter.UserId ?? AnonymousUserId;
        var query = this.appContext.Games.Include(game => game.Owner).Where(game => game.IsActive && game.OwnerId != userId);

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(game => game.Name.Contains(filter.Name));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(game => game.Owner!.City == filter.City);
        }

        if (filter.MaximumPrice.HasValue)
        {
            query = query.Where(game => game.PricePerDay <= filter.MaximumPrice.Value);
        }

        if (filter.PlayerCount.HasValue)
        {
            query = query.Where(game => game.MinimumPlayerNumber <= filter.PlayerCount.Value && game.MaximumPlayerNumber >= filter.PlayerCount.Value);
        }

        if (filter.AvailabilityRange != null)
        {
            var startDateFilter = filter.AvailabilityRange.StartTime;
            var endDateFilter = filter.AvailabilityRange.EndTime;
            query = query.Where(game => !game.Rentals.Any(rental =>
                rental.StartDate.Date <= endDateFilter.Date &&
                rental.EndDate.Date >= startDateFilter.Date));
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets games that are available starting today and continuing through tomorrow.
    /// </summary>
    /// <param name="userId">
    /// Current authenticated user id OR -1 for user not logged in.
    /// Used to exclude the user's own games from the feed.
    /// </param>
    /// <returns>A list of games for the "Available Tonight" section.</returns>
    public async Task<List<Game>> GetGamesForFeedAvailableTonight(int userId)
    {
        var todayDate = DateTime.Today;
        var tomorrowDate = todayDate.AddDays(1);

        return await this.appContext.Games.Include(game => game.Owner).Where(game => game.IsActive && game.OwnerId != userId && !game.Rentals.Any(rental => rental.StartDate.Date <= tomorrowDate && rental.EndDate.Date >= todayDate)).ToListAsync();
    }

    /// <summary>
    /// Gets all remaining active games that are not part of the "Available Tonight" section.
    /// </summary>
    /// <param name="userId">
    /// Current authenticated user id.
    /// Used to exclude the user's own games from the feed.
    /// </param>
    /// <returns>A list of games for the "Available Tonight" section.</returns>
    public async Task<List<Game>> GetRemainingGamesForFeed(int userId)
    {
        var todayDate = DateTime.Today;
        var tomorrowDate = todayDate.AddDays(1);

        return await this.appContext.Games.Where(game => game.IsActive && game.OwnerId != userId && game.Rentals.Any(rental => rental.StartDate.Date <= tomorrowDate && rental.EndDate.Date >= todayDate)).ToListAsync();
    }

    private IQueryable<Game> GamesWithOwner() =>
        this.appContext.Games.Include(game => game.Owner);

    public void AddGame(Game game)
    {
        if (game.Owner != null)
        {
            var owner = this.ResolveUser(game.Owner);
            game.Owner = owner;
            game.OwnerId = owner.PamUserId;
        }

        this.appContext.Games.Add(game);
        this.appContext.SaveChanges();
    }

    public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId)
    {
        return this.GamesWithOwner()
            .Where(game => game.Owner != null && game.Owner.Id == ownerAccountId)
            .ToImmutableList();
    }

    public void UpdateGame(int id, Game updated)
    {
        var existing = this.GamesWithOwner().FirstOrDefault(game => game.Id == id);
        if (existing == null)
        {
            throw new KeyNotFoundException();
        }

        if (updated.Owner != null)
        {
            existing.Owner = this.ResolveUser(updated.Owner);
        }

        existing.Name = updated.Name;
        existing.PricePerDay = updated.PricePerDay;
        existing.MinimumPlayerNumber = updated.MinimumPlayerNumber;
        existing.MaximumPlayerNumber = updated.MaximumPlayerNumber;
        existing.Description = updated.Description;
        existing.Image = updated.Image;
        existing.IsActive = updated.IsActive;

        this.appContext.SaveChanges();
    }

    public Game GetGame(int id)
    {
        var game = this.GamesWithOwner().FirstOrDefault(repositoryGame => repositoryGame.Id == id);
        if (game == null)
        {
            throw new KeyNotFoundException();
        }

        return game;
    }

    public Game DeleteGame(int id)
    {
        var game = this.GamesWithOwner().FirstOrDefault(repositoryGame => repositoryGame.Id == id);
        if (game == null)
        {
            throw new KeyNotFoundException();
        }

        this.appContext.Games.Remove(game);
        this.appContext.SaveChanges();
        return game;
    }

    private static Game ConvertGameDataToGameObject(SqlDataReader reader)
    {
        return new Game
        {
            Id = Convert.ToInt32(reader["game_id"]),
            Name = Convert.ToString(reader["name"]) ?? string.Empty,
            PricePerDay = Convert.ToDecimal(reader["price"]),
            MinimumPlayerNumber = Convert.ToInt32(reader["minimum_player_number"]),
            MaximumPlayerNumber = Convert.ToInt32(reader["maximum_player_number"]),
            Description = Convert.ToString(reader["description"]) ?? string.Empty,
            Image = reader["image"] == DBNull.Value ? null : (byte[])reader["image"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            OwnerId = Convert.ToInt32(reader["owner_id"]),
        };
    }

    /// Gets all active games from the database.
    /// <param name="userId">
    /// Current authenticated user id OR -1 for user not logged in.
    /// Used to exclude the user's own games from the results.
    /// </param>
    /// <returns>A list of all active games.</returns>
    private async Task<List<Game>> GetAllActiveGames(int userId)
    {
        return await this.appContext.Games.Include(game => game.Owner).Where(game => game.IsActive && game.OwnerId != userId).ToListAsync();
    }

    private User ResolveUser(User user)
    {
        if (user == null)
        {
            return null!;
        }

        if (user.PamUserId != 0)
        {
            var tracked = this.appContext.Users.Local.FirstOrDefault(u => u.PamUserId == user.PamUserId)
                         ?? this.appContext.Users.SingleOrDefault(u => u.PamUserId == user.PamUserId);
            if (tracked != null)
            {
                return tracked;
            }

            throw new InvalidOperationException($"User with PamUserId {user.PamUserId} was not found.");
        }

        if (user.Id != Guid.Empty)
        {
            var tracked = this.appContext.Users.Local.FirstOrDefault(u => u.Id == user.Id)
                         ?? this.appContext.Users.SingleOrDefault(u => u.Id == user.Id);
            if (tracked != null)
            {
                return tracked;
            }

            throw new InvalidOperationException($"User with Id {user.Id} was not found.");
        }

        throw new InvalidOperationException("Owner must include a valid PamUserId or Id.");
    }
}
