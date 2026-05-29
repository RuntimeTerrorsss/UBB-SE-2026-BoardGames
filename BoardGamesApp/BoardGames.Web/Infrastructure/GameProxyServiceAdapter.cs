// <copyright file="GameProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using System.Net;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class GameProxyServiceAdapter : IGameProxyService
    {
        private readonly HttpClient httpClient;

        public GameProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync(CancellationToken cancellationToken = default)
            => await this.GetListAsync("games", cancellationToken);

        public async Task<GameDTO?> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"games/{gameId}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var summary = await HttpProxyClient.ReadAsync<GameSummaryDTO>(response, cancellationToken);
            return GameDtoMapper.FromSummary(summary);
        }

        public async Task<IReadOnlyList<GameDTO>> GetGamesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
            => await this.GetListAsync($"games/owner/{ownerId}", cancellationToken);

        public async Task<IReadOnlyList<GameDTO>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => await this.GetListAsync($"games/renter/{renterAccountId}/available", cancellationToken);

        public async Task<IReadOnlyList<GameDTO>> SearchGamesAsync(GameSearchCriteriaDTO criteria, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("games/search", criteria, cancellationToken);
            var summaries = await HttpProxyClient.ReadAsync<List<GameSummaryDTO>>(response, cancellationToken);
            return summaries.Select(GameDtoMapper.FromSummary).ToList();
        }

        public async Task CreateGameAsync(GameDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("games", GameDtoMapper.ToSummary(body), cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdateGameAsync(int gameId, GameDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PutAsJsonAsync($"games/{gameId}", GameDtoMapper.ToSummary(body), cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.DeleteAsync($"games/{gameId}", cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        private async Task<IReadOnlyList<GameDTO>> GetListAsync(string requestPath, CancellationToken cancellationToken)
        {
            using var response = await this.httpClient.GetAsync(requestPath, cancellationToken);
            var summaries = await HttpProxyClient.ReadAsync<List<GameSummaryDTO>>(response, cancellationToken);
            return summaries.Select(GameDtoMapper.FromSummary).ToList();
        }
    }
}
