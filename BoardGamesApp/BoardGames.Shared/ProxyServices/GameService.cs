using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;


namespace BoardGames.Shared.ProxyServices
{
    public sealed class GameService : ApiServiceBase, IGameService
    {
        public GameService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult> CreateGameAsync(GameSummaryDTO game, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsJsonAsync("api/games", game, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult> UpdateGameAsync(int gameId, GameSummaryDTO game, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync($"api/games/{gameId}", game, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<GameSummaryDTO>> DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<GameSummaryDTO>(
                token => client.DeleteAsync($"api/games/{gameId}", token),
                async (response, token) =>
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return await ApiResponseReader.ToFailAsync<GameSummaryDTO>(response, token);
                    }

                    var parsed = await ApiResponseReader.ReadJsonAsync<GameSummaryDTO>(response, token);
                    return parsed.Success ? parsed : ServiceResult<GameSummaryDTO>.Ok(new GameSummaryDTO { Id = gameId });
                },
                cancellationToken);
        }

        public Task<ServiceResult<GameSummaryDTO>> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<GameSummaryDTO>(
                token => client.GetAsync($"api/games/{gameId}", token),
                (response, token) => ApiResponseReader.ReadJsonAsync<GameSummaryDTO>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/games/owner/{ownerAccountId}", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetAllGamesAsync(CancellationToken cancellationToken = default)
            => FetchListAsync("api/games", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/games/renter/{renterAccountId}/available", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetActiveGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/games/owner/{ownerAccountId}/active", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> SearchGamesAsync(
            GameSearchCriteriaDTO criteria,
            CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<GameSummaryDTO>>(
                token => client.PostAsJsonAsync("api/games/search", criteria, token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<GameSummaryDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(parsed.Data ?? new List<GameSummaryDTO>())
                        : ServiceResult<IReadOnlyList<GameSummaryDTO>>.Fail(parsed);
                },
                cancellationToken);
        }

        private Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> FetchListAsync(string requestPath, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<GameSummaryDTO>>(
                token => client.GetAsync(requestPath, token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<GameSummaryDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(parsed.Data ?? new List<GameSummaryDTO>())
                        : ServiceResult<IReadOnlyList<GameSummaryDTO>>.Fail(parsed);
                },
                cancellationToken);
        }
    }
}
