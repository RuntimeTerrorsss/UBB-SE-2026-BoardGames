// <copyright file="IGameService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IGameService
    {
        Task<IReadOnlyList<GameSummaryDTO>> GetAllActiveGames();

        Task<IReadOnlyList<GameSummaryDTO>> GetAvailableGamesForRenter(Guid renterAccountId);

        IReadOnlyList<GameSummaryDTO> GetGamesForOwner(Guid ownerAccountId);

        IReadOnlyList<GameSummaryDTO> GetActiveGamesForOwner(Guid ownerAccountId);

        Task<IReadOnlyList<GameSummaryDTO>> GetAllGamesAdmin();

        Task<GameDetailDTO> GetGameById(int gameId);

        GameDetailDTO CreateGame(GameCreateDTO dto, Guid ownerAccountId);

        void UpdateGame(int gameId, GameUpdateDTO dto, Guid requestingAccountId, bool isAdmin);

        GameDetailDTO DeleteGame(int gameId, Guid requestingAccountId, bool isAdmin);

        Task<IReadOnlyList<GameSummaryDTO>> SearchGames(GameSearchCriteriaDTO criteria);

        Task<byte[]?> GetGameImage(int gameId);
    }
}
