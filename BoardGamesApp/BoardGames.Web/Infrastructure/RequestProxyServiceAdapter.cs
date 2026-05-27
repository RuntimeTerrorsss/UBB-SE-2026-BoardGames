// <copyright file="RequestProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class RequestProxyServiceAdapter : IRequestProxyService
    {
        private readonly IRequestService requestService;

        public RequestProxyServiceAdapter(IRequestService requestService)
        {
            this.requestService = requestService;
        }

        public async Task<IReadOnlyList<RequestDTO>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => (await this.requestService.GetOpenRequestsForOwnerAsync(ownerAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await this.requestService.GetRequestsForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();

        public async Task CreateRequestAsync(CreateRequestDTO body, CancellationToken cancellationToken = default)
            => (await this.requestService.CreateRequestAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task OfferGameAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default)
            => (await this.requestService.OfferGameAsync(requestId, body, cancellationToken)).ThrowIfFailed();

        public async Task DenyRequestAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default)
            => (await this.requestService.DenyRequestAsync(requestId, body, cancellationToken)).ThrowIfFailed();

        public async Task CancelRequestAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default)
            => (await this.requestService.CancelRequestAsync(requestId, body, cancellationToken)).ThrowIfFailed();
    }
}
