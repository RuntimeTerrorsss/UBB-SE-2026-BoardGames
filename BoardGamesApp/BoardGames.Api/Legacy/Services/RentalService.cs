using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Repositories;
using BoardGames.Sharing.DTO;

namespace BoardGames.Api.Legacy.Services
{
    public class RentalService : IRentalService
    {
        private const int MinimumValidDayCount = 1;

        private readonly IRentalRepository rentalRepository;
        private readonly InterfaceGamesRepository gameRepository;

        public RentalService(IRentalRepository rentalRepository, InterfaceGamesRepository gameRepository)
        {
            this.rentalRepository = rentalRepository;
            this.gameRepository = gameRepository;
        }

        public async Task<List<RentalDataTransferObject>> GetRentalsForUser(int userId)
        {
            var rentals = await rentalRepository.GetRentalsForUser(userId);

            return rentals.Select(rental => new RentalDataTransferObject(
                rental.RentalId,
                rental.GameId,
                rental.Game?.Name ?? "Unknown Game",
                rental.ClientId,
                rental.Client?.DisplayName ?? "Unknown Renter",
                rental.OwnerId,
                rental.Owner?.DisplayName ?? "Unknown Owner",
                rental.StartDate,
                rental.EndDate,
                rental.TotalPrice ?? 0m)).ToList();
        }

        public async Task<Rental> GetRentalById(int rentalId)
        {
            return await rentalRepository.GetById(rentalId);
        }

        public async Task<decimal> GetRentalPrice(int rentalId)
        {
            var rental = await rentalRepository.GetById(rentalId);

            if (rental == null)
            {
                return 0m;
            }

            var pricePerDay = await gameRepository.GetPriceGameById(rental.GameId);
            var timeRange = new TimeRange(rental.StartDate, rental.EndDate);

            return await CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);
        }

        public async Task<string> GetGameName(int rentalId)
        {
            var rental = await rentalRepository.GetById(rentalId);

            if (rental == null)
            {
                return "Unknown Rental";
            }

            var game = await gameRepository.GetGameById(rental.GameId);

            if (game == null)
            {
                return "Unknown Game";
            }

            return game.Name;
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await rentalRepository.GetUnavailableTimeRanges(gameId);
        }

        public async Task<bool> CheckGameAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
            {
                return false;
            }

            return await rentalRepository.CheckGameAvailability(startDate.Date, endDate.Date, gameId);
        }

        public async Task<decimal> CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange)
        {
            int days = await CalculateNumberOfDaysInAGivenTimeRange(timeRange);
            return days * price;
        }

        public async Task<int> CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange)
        {
            int days = (selectedTimeRange.EndTime.Date - selectedTimeRange.StartTime.Date).Days + MinimumValidDayCount;
            return days < MinimumValidDayCount ? MinimumValidDayCount : days;
        }

        public async Task<Rental> CreateRental(int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
            {
                throw new ArgumentException("End date must be on or after the start date.");
            }

            startDate = startDate.Date;
            endDate = endDate.Date;

            bool isAvailable = await CheckGameAvailability(gameId, startDate, endDate);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The game is not available for the selected period.");
            }

            var pricePerDay = await gameRepository.GetPriceGameById(gameId);
            var timeRange = new TimeRange(startDate, endDate);
            var totalPrice = await CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);

            var rental = new Rental
            {
                GameId = gameId,
                ClientId = clientId,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = totalPrice,
            };

            await rentalRepository.AddRental(rental);

            return rental;
        }
    }
}
