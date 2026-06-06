using System.Diagnostics;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services;
public class BookingService : InterfaceBookingService
{
    private const int MinimumValidDayCount = 1;
    private readonly InterfaceGamesRepository gamesRepository;
    private readonly IRentalRepository rentalsRepository;
    private readonly IUserRepository usersRepository;
    public BookingService(
        InterfaceGamesRepository gamesRepository,
        IRentalRepository rentalsRepository,
        IUserRepository usersRepository)
    {
        this.gamesRepository = gamesRepository;
        this.rentalsRepository = rentalsRepository;
        this.usersRepository = usersRepository;
    }
    public async Task<BookingDTO> GetBookingInformationForSpecificGame(int gameId)
    {
        try
        {
            var bookedGame = await this.gamesRepository.GetGameById(gameId);
            Debug.WriteLine($"bookedGame: {bookedGame?.Name ?? "NULL"}");

            if (bookedGame == null)
            {
                throw new InvalidOperationException($"Game with id {gameId} was not found.");
            }

            var gameOwner = await this.usersRepository.GetGameById(bookedGame.OwnerId);
            Debug.WriteLine($"gameOwner: {gameOwner?.DisplayName ?? "NULL"}");

            if (gameOwner == null)
            {
                throw new InvalidOperationException($"Owner for game id {gameId} was not found.");
            }

            return new BookingDTO
            {
                GameId = bookedGame.Id,
                Name = bookedGame.Name,
                Image = bookedGame.Image,
                Price = bookedGame.PricePerDay,
                City = gameOwner.City,
                MinimumNrPlayers = bookedGame.MinimumPlayerNumber,
                MaximumNumberPlayers = bookedGame.MaximumPlayerNumber,
                Description = bookedGame.Description,
                UserId = gameOwner.Id,
                DisplayName = gameOwner.DisplayName,
                IsSuspended = gameOwner.IsSuspended,
                AvatarUrl = gameOwner.AvatarUrl,
                CreatedAt = gameOwner.CreatedAt,
            };
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"BOOKING ERROR: {exception.Message}");
            Debug.WriteLine($"INNER: {exception.InnerException?.Message}");
            throw;
        }
    }
    public async Task<TimeRange[]> GetUnavailableTimeRanges(int gameId)
    {
        try
        {
            return (await this.rentalsRepository.GetUnavailableTimeRanges(gameId)).ToArray();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to retrieve unavailable time ranges for game {gameId}.", exception);
        }
    }
    public async Task<bool> CheckGameAvailability(int gameId, TimeRange timeRange)
    {
        try
        {
            return await this.rentalsRepository.CheckGameAvailability(timeRange.StartTime, timeRange.EndTime, gameId);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to check availability for game {gameId}.", exception);
        }
    }
    public decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange)
    {
        int days = (timeRange.EndTime.Date - timeRange.StartTime.Date).Days + MinimumValidDayCount;

        if (days < MinimumValidDayCount)
        {
            days = MinimumValidDayCount;
        }

        return days * price;
    }
    public int CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange)
    {
        int days = (selectedTimeRange.EndTime.Date - selectedTimeRange.StartTime.Date).Days + MinimumValidDayCount;
        return days < MinimumValidDayCount ? MinimumValidDayCount : days;
    }

    public async Task AddBooking(int gameId, int clientId, TimeRange timeRange)
    {
        if (clientId <= 0)
        {
            throw new InvalidOperationException("A valid logged-in renter account is required to complete a booking.");
        }

        try
        {
            await this.rentalsRepository.BookGameWithRentalRequest(
                clientId,
                gameId,
                timeRange.StartTime.Date,
                timeRange.EndTime.Date);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to add booking for game {gameId}.", exception);
        }
    }
}
