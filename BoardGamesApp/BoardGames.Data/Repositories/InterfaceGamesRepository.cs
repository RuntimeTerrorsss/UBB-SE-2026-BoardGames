// <copyright file="InterfaceGamesRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface InterfaceGamesRepository : IRepository<Game>
    {
        // --- Project 1 methods ---

        /// <summary>
        /// Retrieves a list of games that match the specified filter criteria.
        /// </summary>
        /// <param name="filter">The criteria used to filter the games.</param>
        /// <returns>A list of games matching the filter.</returns>
        Task<List<Game>> GetGamesByFilter(FilterCriteria filter);

        /// <summary>
        /// Retrieves a list of games available tonight for the specified user's feed.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of games available tonight.</returns>
        Task<List<Game>> GetGamesForFeedAvailableTonight(int userId);

        /// <summary>
        /// Retrieves a list of other games for the specified user's feed.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of other games for the user's feed.</returns>
        Task<List<Game>> GetRemainingGamesForFeed(int userId);

        Task<decimal> GetPriceGameById(int gameId);

        // --- Project 2 methods (merged from IGameRepository / IGameRepository2.cs) ---
        void AddGame(Game game);

        Game DeleteGame(int id);

        void UpdateGame(int id, Game updated);

        Game GetGame(int id);

        ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId);
    }
}
