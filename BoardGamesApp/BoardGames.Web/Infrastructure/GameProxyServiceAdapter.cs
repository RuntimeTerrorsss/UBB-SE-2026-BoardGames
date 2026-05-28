// <copyright file="GameProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class GameProxyServiceAdapter : IGameProxyService
    {
        private readonly IGameService gameService;

        public GameProxyServiceAdapter(IGameService gameService)
        {
            this.gameService = gameService;
        }

        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync(CancellationToken cancellationToken = default)
        {
            var games = (await this.gameService.GetAllGamesAsync(cancellationToken)).ThrowIfFailed();
            return games.Select(GameDtoMapper.FromSummary).ToList();
        }

        public async Task<GameDTO?> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            var result = await this.gameService.GetGameByIdAsync(gameId, cancellationToken);
            if (result.Success)
            {
                return result.Data is null ? null : GameDtoMapper.FromSummary(result.Data);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw ProxyResultExtensions.ToException(result);
        }

        public async Task<IReadOnlyList<GameDTO>> GetGamesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            var games = (await this.gameService.GetGamesForOwnerAsync(ownerId, cancellationToken)).ThrowIfFailed();
            return games.Select(GameDtoMapper.FromSummary).ToList();
        }

        public async Task<IReadOnlyList<GameDTO>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
        {
            var games = (await this.gameService.GetAvailableGamesForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();
            return games.Select(GameDtoMapper.FromSummary).ToList();
        }

        public async Task<IReadOnlyList<GameDTO>> SearchGamesAsync(GameSearchCriteriaDTO criteria, CancellationToken cancellationToken = default)
        {
            var games = (await this.gameService.SearchGamesAsync(criteria, cancellationToken)).ThrowIfFailed();
            return games.Select(GameDtoMapper.FromSummary).ToList();
        }

        public async Task CreateGameAsync(GameDTO body, CancellationToken cancellationToken = default)
            => (await this.gameService.CreateGameAsync(GameDtoMapper.ToSummary(body), cancellationToken)).ThrowIfFailed();

        public async Task UpdateGameAsync(int gameId, GameDTO body, CancellationToken cancellationToken = default)
            => (await this.gameService.UpdateGameAsync(gameId, GameDtoMapper.ToSummary(body), cancellationToken)).ThrowIfFailed();

        public async Task DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
            => (await this.gameService.DeleteGameAsync(gameId, cancellationToken)).ThrowIfFailed();
    }
}
