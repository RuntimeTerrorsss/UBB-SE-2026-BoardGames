// <copyright file="IGameService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IGameService
    {
        Task<ServiceResult> CreateGameAsync(GameSummaryDTO game, CancellationToken cancellationToken = default);

        Task<ServiceResult> CreateGameAsync(GameCreateDTO game, CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateGameAsync(int gameId, GameSummaryDTO game, CancellationToken cancellationToken = default);

        Task<ServiceResult<GameSummaryDTO>> DeleteGameAsync(int gameId, CancellationToken cancellationToken = default);

        Task<ServiceResult<GameSummaryDTO>> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default);

        Task<ServiceResult<GameDetailDTO>> GetGameDetailsByIdAsync(int gameId, CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult<GameDetailDTO>.Fail("Game details are not implemented by this client."));

        Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetAllGamesAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetActiveGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> SearchGamesAsync(GameSearchCriteriaDTO criteria, CancellationToken cancellationToken = default);
    }
}
