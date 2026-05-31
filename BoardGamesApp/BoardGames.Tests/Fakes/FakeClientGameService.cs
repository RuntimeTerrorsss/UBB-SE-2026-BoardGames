//// <copyright file="FakeClientGameService.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using BoardGames.Shared.DTO;
//using BoardGames.Shared.ProxyServices;

//namespace BoardGames.Tests.Fakes
//{
//    internal sealed class FakeClientGameService : IGameService
//    {
//        public ServiceResult<IReadOnlyList<GameSummaryDTO>> AllGamesResult { get; set; }
//            = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(Array.Empty<GameSummaryDTO>());

//        public ServiceResult<IReadOnlyList<GameSummaryDTO>> SearchGamesResult { get; set; }
//            = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(Array.Empty<GameSummaryDTO>());

//        public int GetAllGamesCallCount { get; private set; }

//        public int SearchGamesCallCount { get; private set; }

//        public GameSearchCriteriaDTO? LastSearchCriteria { get; private set; }

//        public Task<ServiceResult> CreateGameAsync(GameSummaryDTO game, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult.Ok());
//        }

//        public Task<ServiceResult> UpdateGameAsync(int gameId, GameSummaryDTO game, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult.Ok());
//        }

//        public Task<ServiceResult<GameSummaryDTO>> DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult<GameSummaryDTO>.Ok(new GameSummaryDTO { Id = gameId }));
//        }

//        public Task<ServiceResult<GameSummaryDTO>> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult<GameSummaryDTO>.Ok(new GameSummaryDTO { Id = gameId }));
//        }

//        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(Array.Empty<GameSummaryDTO>()));
//        }

//        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetAllGamesAsync(CancellationToken cancellationToken = default)
//        {
//            this.GetAllGamesCallCount++;
//            return Task.FromResult(this.AllGamesResult);
//        }

//        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> SearchGamesAsync(GameSearchCriteriaDTO criteria, CancellationToken cancellationToken = default)
//        {
//            this.SearchGamesCallCount++;
//            this.LastSearchCriteria = criteria;
//            return Task.FromResult(this.SearchGamesResult);
//        }

//        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(Array.Empty<GameSummaryDTO>()));
//        }

//        public Task<ServiceResult<IReadOnlyList<GameSummaryDTO>>> GetActiveGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(Array.Empty<GameSummaryDTO>()));
//        }
//    }
//}
