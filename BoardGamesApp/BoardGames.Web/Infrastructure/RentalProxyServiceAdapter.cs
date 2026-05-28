// <copyright file="RentalProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class RentalProxyServiceAdapter : IRentalProxyService
    {
        private readonly HttpClient httpClient;

        public RentalProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"rentals/owner/{ownerAccountId}", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<RentalDTO>>(response, cancellationToken);
        }

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"rentals/renter/{renterAccountId}", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<RentalDTO>>(response, cancellationToken);
        }
    }
}
