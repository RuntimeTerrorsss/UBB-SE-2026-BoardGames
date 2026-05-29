// <copyright file="RentalService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Api.Mappers;
using BoardGames.Data.Constants;
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
            this.rentalDataRepository = rentalRepository;
            this.gameLookupRepository = gameRepository;
            this.rentalDtoMapper = rentalMapper;
        }

        public bool IsSlotAvailable(int gameId, DateTime startDate, DateTime endDate)
        {
            foreach (var rental in this.rentalDataRepository.GetRentalsByGame(gameId))
            {
                if (startDate < rental.EndDate.AddHours(DomainConstants.RentalBufferHours) && endDate > rental.StartDate.AddHours(-DomainConstants.RentalBufferHours))
                {
                    return false;
                }
            }

            return true;
        }

        public ImmutableList<BookedDateRangeDTO> GetBookedDatesForGame(int gameId)
        {
            return this.rentalDataRepository.GetRentalsByGame(gameId)
                .Select(rental => new BookedDateRangeDTO
                {
                    StartDate = rental.StartDate,
                    EndDate = rental.EndDate,
                })
                .ToImmutableList();
        }

        public void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            if (!DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate))
            {
                throw new ArgumentException("Start date must be before end date and not in the past.");
            }

            var game = this.gameLookupRepository.GetGame(gameId);
            if (game.Owner?.Id != ownerAccountId)
            {
                throw new InvalidOperationException("Seller ID must match Game Owner ID [ENT-REN-04].");
            }

            if (!this.IsSlotAvailable(gameId, startDate, endDate))
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
            this.rentalDataRepository.AddConfirmed(rental);
        }

        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) =>
            this.rentalDataRepository.GetRentalsByRenter(renterAccountId).Select(rental => this.rentalDtoMapper.ToDTO(rental)!).ToImmutableList();

        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) =>
            this.rentalDataRepository.GetRentalsByOwner(ownerAccountId).Select(rental => this.rentalDtoMapper.ToDTO(rental)!).ToImmutableList();
    }
}
