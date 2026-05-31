// <copyright file="GameService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Mappers;
using BoardGames.Data.Constants;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class GameService : IGameService
    {
        private const int NoActiveOrUpcomingRentals = 0;
        private const int SingularRentalCount = 1;

        private readonly InterfaceGamesRepository gameListingRepository;
        private readonly IRentalRepository gameRentalRepository;
        private readonly GameMapper gameDtoMapper;
        private readonly IRequestService rentalRequestService;

        public GameService(InterfaceGamesRepository gameRepository, IRentalRepository rentalRepository, GameMapper gameMapper, IRequestService requestService)
        {
            this.gameListingRepository = gameRepository;
            this.gameRentalRepository = rentalRepository;
            this.gameDtoMapper = gameMapper;
            this.rentalRequestService = requestService;
        }

        private List<string> ValidateGameInput(string name, decimal price, int minPlayers, int maxPlayers, string description) =>
            GameInputHelper.BuildValidationErrors(
                name,
                price,
                minPlayers,
                maxPlayers,
                description,
                DomainConstants.GameMinimumNameLength,
                DomainConstants.GameMaximumNameLength,
                DomainConstants.GameMinimumAllowedPrice,
                DomainConstants.GameMinimumPlayerCount,
                DomainConstants.GameMinimumDescriptionLength,
                DomainConstants.GameMaximumDescriptionLength);

        public async Task<IReadOnlyList<GameSummaryDTO>> GetAllActiveGames()
        {
            var games = await gameListingRepository.GetAll();
            return games.Select(game => gameDtoMapper.ToSummaryDTO(game)).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<GameSummaryDTO>> GetAvailableGamesForRenter(Guid renterAccountId)
        {
            var allActive = await GetAllActiveGames();
            return allActive.Where(game => game.OwnerAccountId != renterAccountId).ToList().AsReadOnly();
        }

        public IReadOnlyList<GameSummaryDTO> GetGamesForOwner(Guid ownerAccountId)
        {
            return gameListingRepository.GetGamesByOwner(ownerAccountId)
                .Select(game => gameDtoMapper.ToSummaryDTO(game))
                .ToList().AsReadOnly();
        }

        public IReadOnlyList<GameSummaryDTO> GetActiveGamesForOwner(Guid ownerAccountId)
        {
            return GetGamesForOwner(ownerAccountId)
                .Where(game => game.IsActive).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<GameSummaryDTO>> GetAllGamesAdmin()
        {
            var games = await gameListingRepository.GetAllIncludingInactive();
            return games.Select(game => gameDtoMapper.ToSummaryDTO(game)).ToList().AsReadOnly();
        }

        public async Task<GameDetailDTO> GetGameById(int gameId)
        {
            var game = await this.gameListingRepository.GetGameById(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");
            }

            return this.gameDtoMapper.ToDetailDTO(game);
        }

        public async Task<byte[]?> GetGameImage(int gameId)
        {
            var game = await this.gameListingRepository.GetGameById(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");
            }

            return game.Image;
        }

        public GameDetailDTO CreateGame(GameCreateDTO dto, Guid ownerAccountId)
        {
            var errors = this.ValidateGameInput(dto.Name, dto.Price, dto.MinimumPlayerNumber, dto.MaximumPlayerNumber, dto.Description);
            if (errors.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            dto.Image = GameInputHelper.EnsureImageOrDefault(dto.Image, AppDomain.CurrentDomain.BaseDirectory);
            var model = this.gameDtoMapper.ToModel(dto, ownerAccountId);
            this.gameListingRepository.AddGame(model);

            return this.gameDtoMapper.ToDetailDTO(model);
        }

        public void UpdateGame(int gameId, GameUpdateDTO dto, Guid requestingAccountId, bool isAdmin)
        {
            var game = this.gameListingRepository.GetGame(gameId);
            if (game.Owner?.Id != requestingAccountId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this game.");
            }

            var errors = this.ValidateGameInput(dto.Name, dto.Price, dto.MinimumPlayerNumber, dto.MaximumPlayerNumber, dto.Description);
            if (errors.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            dto.Image = GameInputHelper.EnsureImageOrDefault(dto.Image, AppDomain.CurrentDomain.BaseDirectory);
            this.gameDtoMapper.ApplyUpdate(game, dto);
            this.gameListingRepository.UpdateGame(gameId, game);
        }

        public GameDetailDTO DeleteGame(int gameId, Guid requestingAccountId, bool isAdmin)
        {
            var game = this.gameListingRepository.GetGame(gameId);
            if (game.Owner?.Id != requestingAccountId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

            var gameRentals = this.gameRentalRepository.GetRentalsByGame(gameId);
            var now = DateTime.Now;
            var activeCount = gameRentals.Count(rental => rental.EndDate >= now);
            if (activeCount > NoActiveOrUpcomingRentals)
            {
                var word = activeCount == SingularRentalCount ? "rental" : "rentals";
                throw new InvalidOperationException($"There are {activeCount} active {word} for this game and it cannot be removed now.");
            }

            foreach (var rental in gameRentals)
            {
                this.gameRentalRepository.Delete(rental.Id);
            }

            this.rentalRequestService.OnGameDeactivated(gameId);
            this.gameListingRepository.DeleteGame(gameId);

            return this.gameDtoMapper.ToDetailDTO(game);
        }

        public async Task<IReadOnlyList<GameSummaryDTO>> SearchGames(GameSearchCriteriaDTO criteria)
        {
            var filter = new FilterCriteria
            {
                Name = criteria.Name,
                City = criteria.City,
                MaximumPrice = criteria.MaximumPrice,
                PlayerCount = criteria.PlayerCount,
                AvailabilityRange = criteria.AvailableFrom.HasValue && criteria.AvailableTo.HasValue
                    ? new TimeRange(criteria.AvailableFrom.Value, criteria.AvailableTo.Value)
                    : null
            };

            if (!string.IsNullOrEmpty(criteria.SortBy))
            {
                if (Enum.TryParse<SortOption>(criteria.SortBy, true, out var sortOption))
                {
                    filter.SortOption = sortOption;
                }
            }

            var games = await this.gameListingRepository.GetGamesByFilter(filter);
            var summaries = games.Select(g => this.gameDtoMapper.ToSummaryDTO(g)).ToList();

            if (criteria.ExcludeOwnerAccountId.HasValue)
            {
                summaries = summaries
                    .Where(g => g.OwnerAccountId != criteria.ExcludeOwnerAccountId.Value)
                    .ToList();
            }

            return summaries.AsReadOnly();
        }
    }
}
