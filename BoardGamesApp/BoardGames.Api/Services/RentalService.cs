using System;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Data.Constants;
using BoardGames.Api.Mappers;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class RentalService : IRentalService
    {
        private const int NewRentalId = 0;

        private readonly IRentalRepository rentalDataRepository;
        private readonly IGameRepository gameLookupRepository;
        private readonly RentalMapper rentalDtoMapper;

        public RentalService(IRentalRepository rentalRepository, IGameRepository gameRepository, RentalMapper rentalMapper)
        {
            rentalDataRepository = rentalRepository;
            gameLookupRepository = gameRepository;
            rentalDtoMapper = rentalMapper;
        }

        public bool IsSlotAvailable(int gameId, DateTime startDate, DateTime endDate)
        {
            foreach (var rental in rentalDataRepository.GetRentalsByGame(gameId))
            {
                if (startDate < rental.EndDate.AddHours(DomainConstants.RentalBufferHours) && endDate > rental.StartDate.AddHours(-DomainConstants.RentalBufferHours))
                {
                    return false;
                }
            }

            return true;
        }

        public void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            if (!DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate))
            {
                throw new ArgumentException("Start date must be before end date and not in the past.");
            }

            var game = gameLookupRepository.GetGame(gameId);
            if (game.Owner?.Id != ownerAccountId)
            {
                throw new InvalidOperationException("Seller ID must match Game Owner ID [ENT-REN-04].");
            }

            if (!IsSlotAvailable(gameId, startDate, endDate))
            {
                throw new InvalidOperationException($"Selected dates fall within the mandatory {DomainConstants.RentalBufferHours}-hour buffer of another rental.");
            }

            var rental = new Rental
            {
                Game = new Game { Id = gameId },
                Client = new Account { Id = renterAccountId },
                Owner = new Account { Id = ownerAccountId },
                StartDate = startDate,
                EndDate = endDate,
            };
            rentalDataRepository.AddConfirmed(rental);
        }

        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) =>
            rentalDataRepository.GetRentalsByRenter(renterAccountId).Select(rental => rentalDtoMapper.ToDTO(rental)!).ToImmutableList();

        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) =>
            rentalDataRepository.GetRentalsByOwner(ownerAccountId).Select(rental => rentalDtoMapper.ToDTO(rental)!).ToImmutableList();

        public Task<Rental?> GetRentalById(int rentalId)
        {
            try
            {
                return Task.FromResult<Rental?>(rentalDataRepository.Get(rentalId));
            }
            catch (KeyNotFoundException)
            {
                return Task.FromResult<Rental?>(null);
            }
        }

        public Task<decimal> GetRentalPrice(int rentalId)
        {
            var rental = rentalDataRepository.Get(rentalId);
            if (rental.TotalPrice.HasValue)
            {
                return Task.FromResult(rental.TotalPrice.Value);
            }

            var game = rental.Game ?? gameLookupRepository.GetGame(rental.GameId);
            var days = Math.Max(1, (rental.EndDate.Date - rental.StartDate.Date).Days + 1);
            return Task.FromResult(game.PricePerDay * days);
        }

        public Task<string> GetGameName(int rentalId)
        {
            var rental = rentalDataRepository.Get(rentalId);
            var game = rental.Game ?? gameLookupRepository.GetGame(rental.GameId);
            return Task.FromResult(game.Name);
        }

        public async Task<List<RentalDTO>> GetRentalsForUser(int userId)
        {
            var rentals = await rentalDataRepository.GetRentalsForUser(userId);
            return rentals.Select(rental => rentalDtoMapper.ToDTO(rental)!).ToList();
        }
    }
}
