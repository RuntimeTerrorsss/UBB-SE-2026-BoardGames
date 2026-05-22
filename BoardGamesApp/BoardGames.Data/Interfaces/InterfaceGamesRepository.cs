using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardGames.Data.Enum;

namespace BookingBoardGames.Data.Interfaces
{
    public interface InterfaceGamesRepository : IRepository<Game>
    {
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
    }
}
