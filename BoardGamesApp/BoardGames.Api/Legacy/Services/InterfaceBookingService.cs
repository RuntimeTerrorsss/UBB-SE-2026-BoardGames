using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services
{
    public interface InterfaceBookingService
    {
        Task<BookingDTO> GetBookingInformationForSpecificGame(int gameId);
        Task<TimeRange[]> GetUnavailableTimeRanges(int gameId);
        Task<bool> CheckGameAvailability(int gameId, TimeRange range);
        decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange);
        int CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange);
        Task AddBooking(int gameId, int clientId, TimeRange timeRange);
    }
}
