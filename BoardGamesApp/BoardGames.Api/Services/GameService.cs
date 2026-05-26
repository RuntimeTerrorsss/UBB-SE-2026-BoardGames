using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Constants;
using BoardGames.Data.Enums;
using BoardGames.Api.Mappers;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Data.Models;

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
            gameListingRepository = gameRepository;
            gameRentalRepository = rentalRepository;
            gameDtoMapper = gameMapper;
            rentalRequestService = requestService;
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
            return games.Select(g => gameDtoMapper.ToSummaryDTO(g)).ToList().AsReadOnly();
        }

        public IReadOnlyList<GameSummaryDTO> GetGamesForOwner(Guid ownerAccountId)
        {
            return gameListingRepository.GetGamesByOwner(ownerAccountId)
                .Select(g => gameDtoMapper.ToSummaryDTO(g))
                .ToList().AsReadOnly();
        }

        public IReadOnlyList<GameSummaryDTO> GetActiveGamesForOwner(Guid ownerAccountId)
        {
            return GetGamesForOwner(ownerAccountId)
                .Where(g => g.IsActive).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<GameSummaryDTO>> GetAllGamesAdmin()
        {
            var games = await gameListingRepository.GetAllIncludingInactive();
            return games.Select(g => gameDtoMapper.ToSummaryDTO(g)).ToList().AsReadOnly();
        }

        public async Task<GameDetailDTO> GetGameById(int gameId)
        {
            var game = await gameListingRepository.GetGameById(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");
            }
            return gameDtoMapper.ToDetailDTO(game);
        }

        public async Task<byte[]?> GetGameImage(int gameId)
        {
            var game = await gameListingRepository.GetGameById(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");
            }
            return game.Image;
        }

        public GameDetailDTO CreateGame(GameCreateDTO dto, Guid ownerAccountId)
        {
            var errors = ValidateGameInput(dto.Name, dto.Price, dto.MinimumPlayerNumber, dto.MaximumPlayerNumber, dto.Description);
            if (errors.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            dto.Image = GameInputHelper.EnsureImageOrDefault(dto.Image, AppDomain.CurrentDomain.BaseDirectory);
            var model = gameDtoMapper.ToModel(dto, ownerAccountId);
            gameListingRepository.AddGame(model);
            
            return gameDtoMapper.ToDetailDTO(model);
        }

        public void UpdateGame(int gameId, GameUpdateDTO dto, Guid requestingAccountId, bool isAdmin)
        {
            var game = gameListingRepository.GetGame(gameId);
            if (game.Owner?.Id != requestingAccountId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this game.");
            }

            var errors = ValidateGameInput(dto.Name, dto.Price, dto.MinimumPlayerNumber, dto.MaximumPlayerNumber, dto.Description);
            if (errors.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            dto.Image = GameInputHelper.EnsureImageOrDefault(dto.Image, AppDomain.CurrentDomain.BaseDirectory);
            gameDtoMapper.ApplyUpdate(game, dto);
            gameListingRepository.UpdateGame(gameId, game);
        }

        public GameDetailDTO DeleteGame(int gameId, Guid requestingAccountId, bool isAdmin)
        {
            var game = gameListingRepository.GetGame(gameId);
            if (game.Owner?.Id != requestingAccountId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

            var gameRentals = gameRentalRepository.GetRentalsByGame(gameId);
            var now = DateTime.Now;
            var activeCount = gameRentals.Count(rental => rental.EndDate >= now);
            if (activeCount > NoActiveOrUpcomingRentals)
            {
                var word = activeCount == SingularRentalCount ? "rental" : "rentals";
                throw new InvalidOperationException($"There are {activeCount} active {word} for this game and it cannot be removed now.");
            }

            foreach (var rental in gameRentals)
            {
                gameRentalRepository.Delete(rental.Id);
            }

            rentalRequestService.OnGameDeactivated(gameId);
            gameListingRepository.DeleteGame(gameId);
            
            return gameDtoMapper.ToDetailDTO(game);
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

            var games = await gameListingRepository.GetGamesByFilter(filter);
            return games.Select(g => gameDtoMapper.ToSummaryDTO(g)).ToList().AsReadOnly();
        }
    }
}
