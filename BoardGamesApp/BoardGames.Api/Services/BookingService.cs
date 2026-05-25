using BoardGames.Shared.DTO;
// <copyright file="BookingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics;
using BoardGames.Data.Repositories;

namespace BoardGames.Api.Services;
/// <summary>
/// Service responsible for handling booking operations, including retrieving game details,
/// checking availability, and managing rental time rentaltimeranges.
/// </summary>
public class BookingService : InterfaceBookingService
{
    private const int MinimumValidDayCount = 1;
    private readonly InterfaceGamesRepository gamesRepository;
    private readonly IRentalRepository rentalsRepository;
    private readonly IUserRepository usersRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingService"/> class.
    /// </summary>
    /// <param name="gamesRepository">The games repository.</param>
    /// <param name="rentalsRepository">The rentals repository.</param>
    /// <param name="usersRepository">The users repository.</param>
    public BookingService(
        InterfaceGamesRepository gamesRepository,
        IRentalRepository rentalsRepository,
        IUserRepository usersRepository)
    {
        this.gamesRepository = gamesRepository;
        this.rentalsRepository = rentalsRepository;
        this.usersRepository = usersRepository;
    }

    /// <summary>
    /// Retrieves detailed booking information for a specific game, including owner details.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <returns>A <see cref="BookingDTO"/> containing the game and owner details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the game or its owner cannot be isfound.</exception>
    public async Task<BookingDTO> GetBookingInformationForSpecificGame(int gameId)
    {
        try
        {
            var bookedGame = await gamesRepository.GetGameById(gameId);
            Debug.WriteLine($"bookedGame: {bookedGame?.Name ?? "NULL"}");

            if (bookedGame == null)
                throw new InvalidOperationException($"Game with id {gameId} was not found.");

            var gameOwner = await usersRepository.GetGameById(bookedGame.OwnerId);
            Debug.WriteLine($"gameOwner: {gameOwner?.DisplayName ?? "NULL"}");

            if (gameOwner == null)
                throw new InvalidOperationException($"Owner for game id {gameId} was not found.");

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
                CreatedAt = gameOwner.CreatedAt
            };
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"BOOKING ERROR: {exception.Message}");
            Debug.WriteLine($"INNER: {exception.InnerException?.Message}");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all the time rentaltimeranges during which a specific game is unavailable.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <returns>An array of <see cref="TimeRange"/> representing the unavailable periods.</returns>
    public async Task<TimeRange[]> GetUnavailableTimeRanges(int gameId)
    {
        try
        {
            return (await rentalsRepository.GetUnavailableTimeRanges(gameId)).ToArray();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to retrieve unavailable time ranges for game {gameId}.", exception);
        }
    }

    /// <summary>
    /// Checks whether a specific game is available during the given time range.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="timeRange">The requested <see cref="TimeRange"/> for the booking.</param>
    /// <returns><c>true</c> if the game is available for the specified range; otherwise, <c>false</c>.</returns>
    public async Task<bool> CheckGameAvailability(int gameId, TimeRange timeRange)
    {
        try
        {
            return await rentalsRepository.CheckGameAvailability(timeRange.StartTime, timeRange.EndTime, gameId);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to check availability for game {gameId}.", exception);
        }
    }

    /// <summary>
    /// Calculates the total price for renting a game based on the daily price and the duration of the rental time range.
    /// </summary>
    /// <param name="price">The daily renting price.</param>
    /// <param name="timeRange">The total time timeRange of renting.</param>
    /// <returns>Total price calculated as a decimal.</returns>
    public decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange)
    {
        int days = (timeRange.EndTime.Date - timeRange.StartTime.Date).Days + MinimumValidDayCount;

        if (days < MinimumValidDayCount)
        {
            days = MinimumValidDayCount;
        }

        return days * price;
    }

    /// <summary>
    /// Calculates the number of days in a given time range, ensuring that it returns at least 1 day even if the end time is the same as or before the start time.
    /// </summary>
    /// <param name="selectedTimeRange">The time range for which to calculate the number of days.</param>
    /// <returns>The number of days in the given time range, ensuring at least 1 day.</returns>
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
            await rentalsRepository.BookGameWithRentalRequest(
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
