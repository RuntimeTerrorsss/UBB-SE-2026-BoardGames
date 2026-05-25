using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;

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
            => (await requestService.GetOpenRequestsForOwnerAsync(ownerAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await requestService.GetRequestsForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();

        public async Task CreateRequestAsync(CreateRequestDataTransferObject body, CancellationToken cancellationToken = default)
            => (await requestService.CreateRequestAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task OfferGameAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
            => (await requestService.OfferGameAsync(requestId, body, cancellationToken)).ThrowIfFailed();

        public async Task DenyRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
            => (await requestService.DenyRequestAsync(requestId, body, cancellationToken)).ThrowIfFailed();

        public async Task CancelRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
            => (await requestService.CancelRequestAsync(requestId, body, cancellationToken)).ThrowIfFailed();
    }
}
