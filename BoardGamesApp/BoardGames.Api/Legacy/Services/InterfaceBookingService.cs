// <copyright file="InterfaceBookingService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services
{
    /// <summary>
    /// Defines the operations for managing and querying game bookings.
    /// </summary>
    public interface InterfaceBookingService
    {
        /// <summary>
        /// Retrieves the booking details for a specific game.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>A <see cref="BookingDTO"/> containing the game details.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the game or its owner cannot be isfound, or when retrieval fails.
        /// </exception>
        Task<BookingDTO> GetBookingInformationForSpecificGame(int gameId);

        /// <summary>
        /// Retrieves all unavailable time rentaltimeranges for a specific game.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>An array of <see cref="TimeRange"/> representing periods when the game is unavailable.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when retrieval of unavailable rentaltimeranges fails.
        /// </exception>
        Task<TimeRange[]> GetUnavailableTimeRanges(int gameId);

        /// <summary>
        /// Checks if a game is available for booking during a specified time range.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <param name="range">The time range to check for availability.</param>
        /// <returns><c>true</c> if the game is available during the specified range; otherwise, <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the availability check fails.
        /// </exception>
        Task<bool> CheckGameAvailability(int gameId, TimeRange range);

        /// <summary>
        /// Calculates the total rental price for a specific game based on the provided base price and rental time
        /// range.
        /// </summary>
        /// <param name="price">The base price of the game to rent. Must be a non-negative value representing the price per rental period.</param>
        /// <param name="timeRange">The time range for which the game will be rented. Specifies the start and end times of the rental period.</param>
        /// <returns>The total price to be paid for renting the specified game over the given time range.</returns>
        decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange);

        /// <summary>
        /// Calculated the total number of days within a given time range.
        /// </summary>
        /// <param name="selectedTimeRange">The time range for which to calculate the number of days.</param>
        /// <returns>The total number of days within the specified time range.</returns>
        int CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange);

        /// <summary>
        /// Adds a booking for the specified game by the renting user and notifies the listing owner via a rental-request message in chat.
        /// </summary>
        /// <remarks>If the specified time range overlaps with an existing booking for the same game, the
        /// booking will not be added.</remarks>
        /// <param name="gameId">The unique identifier of the game to be booked.</param>
        /// <param name="clientId">The logged-in user's id (renter).</param>
        /// <param name="timeRange">The requested rental period.</param>
        Task AddBooking(int gameId, int clientId, TimeRange timeRange);
    }
}
