// <copyright file="GamesRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames.Data;
using BoardGames.Data.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories;
/// <summary>
/// Repository responsible for reading game/listing data from the database.
/// Important:
/// - This repository only reads data.
/// - It is used by the service layer, not directly by the UI.
/// How ADO.NET handles connections:
/// - When you write using var connection = new SqlConnection(...) and call .Open(), Microsoft checks the pool, so the pool of connections is handled by .net
/// - If there is a free connection, it gives it to you.
/// - When your "using" block finishes, it calls .Close().
/// - Microsoft intercepts your .Close() command. It doesn't actually destroy the connection to the database. It just wipes the data clean and parks it back in the hidden pool for.  the next person to use.
/// </summary>
public class GamesRepository : InterfaceGamesRepository
{
    /// <summary>
    /// Represents the ID used for unauthenticated users.
    /// </summary>
    public const int AnonymousUserId = -1;

    private readonly AppDbContext appContext;

    public GamesRepository(AppDbContext context)
    {
        appContext = context;
    }

    /// <summary>
    /// Gets a single game by its database id.
    /// </summary>
    /// <param name="gameId">The unique id of the game.</param>
    /// <returns>The game object if found; otherwise, null.</returns>
    /// <remarks>
    /// Use this when you already know the exact game id and need full game details.
    /// </remarks>
    public async Task<Game?> GetGameById(int gameId)
    {
        return await appContext.Games.FirstOrDefaultAsync(game => game.Id == gameId);
    }

    public async Task<decimal> GetPriceGameById(int gameId)
    {
        return await appContext.Games.Where(game => game.Id == gameId).Select(game => game.PricePerDay).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all active games that are visible in the system.
    /// </summary>
    /// <returns>A list of all active games.</returns>
    public async Task<List<Game>> GetAll()
    {
        return await GetAllActiveGames(AnonymousUserId);
    }

    /// <summary>
    /// Gets games that match the provided filter criteria.
    /// </summary>
    /// <param name="filter">
    /// Object containing user-entered search/filter values.
    /// All fields may be empty/null.
    /// </param>
    /// <returns>A list of games matching the filter.</returns>
    /// <remarks>
    /// Use this for:
    /// - search page
    /// - filter panel
    /// - search + filters combined
    /// Behavior:
    /// - null/empty fields are ignored
    /// - only active games are returned
    /// - user's own games are excluded if UserId is provided
    /// - if an availability range is provided, only games available in that range are returned.
    /// </remarks>
    public async Task<List<Game>> GetGamesByFilter(FilterCriteria filter)
    {
        var userId = filter.UserId ?? AnonymousUserId;
        var query = appContext.Games.Include(game => game.Owner).Where(game => game.IsActive && game.OwnerId != userId);

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

        return await appContext.Games.Include(game => game.Owner).Where(game => game.IsActive && game.OwnerId != userId && !game.Rentals.Any(rental => rental.StartDate.Date <= tomorrowDate && rental.EndDate.Date >= todayDate)).ToListAsync();
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

        return await appContext.Games.Where(game => game.IsActive && game.OwnerId != userId && game.Rentals.Any(rental => rental.StartDate.Date <= tomorrowDate && rental.EndDate.Date >= todayDate)).ToListAsync();
    }

    // Used to convert game data to Game object
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
        return await appContext.Games.Include(game => game.Owner).Where(game => game.IsActive && game.OwnerId != userId).ToListAsync();
    }
}
