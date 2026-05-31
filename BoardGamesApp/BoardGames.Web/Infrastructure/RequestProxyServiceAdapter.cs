// <copyright file="RequestProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class RequestProxyServiceAdapter : IRequestProxyService
    {
        private readonly HttpClient httpClient;

        public RequestProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IReadOnlyList<RequestDTO>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"requests/owner/{ownerAccountId}/open", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<RequestDTO>>(response, cancellationToken);
        }

        public async Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"requests/renter/{renterAccountId}", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<RequestDTO>>(response, cancellationToken);
        }

        public async Task CreateRequestAsync(CreateRequestDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("requests", body, cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task OfferGameAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PutAsJsonAsync($"requests/{requestId}/offer", body, cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DenyRequestAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PutAsJsonAsync($"requests/{requestId}/deny", body, cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task CancelRequestAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PutAsJsonAsync($"requests/{requestId}/cancel", body, cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }
    }
}
