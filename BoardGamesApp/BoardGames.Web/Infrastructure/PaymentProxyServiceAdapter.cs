// <copyright file="PaymentProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Web.Infrastructure
{
    public sealed class PaymentProxyServiceAdapter : IPaymentProxyService
    {
        private readonly HttpClient httpClient;

        public PaymentProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IReadOnlyList<PaymentDTO>> GetPaymentHistoryForUserAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"api/payments/user/{accountId}/history", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<PaymentDTO>>(response, cancellationToken);
        }
    }
}
