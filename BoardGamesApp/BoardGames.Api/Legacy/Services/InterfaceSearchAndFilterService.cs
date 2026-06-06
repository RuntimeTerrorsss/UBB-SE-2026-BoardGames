using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services
{
    public interface InterfaceSearchAndFilterService
    {
        Task<GameDTO[]> SearchGamesByFilter(FilterCriteria filter);
        Task<GameDTO[]> GetGamesFeedAvailableTonightByUser(int userId);
        Task<GameDTO[]> GetOtherGamesFeedByUser(int userId);
        Task<GameDTO[]> ApplyFilters(GameDTO[] games, FilterCriteria filter);
        Task<(List<GameDTO> AvailableTonight, List<GameDTO> Others, int TotalAvailableGamesCount)>
        GetDiscoveryFeedPaged(int userId, int page, int pageSize);
        bool IsValidDateRange(DateTime? start, DateTime? end);
        bool IsValidPlayersCount(int? players);
        void UpdateFilterFromUI(FilterCriteria filter, double selectedMaxPrice, double selectedMinimumPlayerCount, DateTime? selectedStartDate, DateTime? selectedEndDate);
    }
}
