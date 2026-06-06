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

        public async Task<IReadOnlyList<BookedDateRangeDTO>> GetBookedDatesForGameAsync(int gameId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"rentals/games/{gameId}/booked-dates", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<BookedDateRangeDTO>>(response, cancellationToken);
        }

        public async Task<bool> CheckAvailabilityAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var url = $"rentals/games/{gameId}/availability?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            using var response = await this.httpClient.GetAsync(url, cancellationToken);
            return await HttpProxyClient.ReadAsync<bool>(response, cancellationToken);
        }

        public async Task<RentalCheckoutDTO?> GetCheckoutSummaryAsync(int rentalId, Guid renterAccountId, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"rentals/{rentalId}/checkout?accountId={renterAccountId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await HttpProxyClient.ReadAsync<RentalCheckoutDTO>(response, cancellationToken);
        }

        public async Task CompleteCardPaymentAsync(CompleteRentalCardPaymentDTO payment, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("rentals/complete-card-payment", payment, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string message = response.ReasonPhrase ?? "Payment failed.";
                try
                {
                    var payload = await response.Content.ReadFromJsonAsync<ApiErrorPayload>(cancellationToken: cancellationToken);
                    if (!string.IsNullOrWhiteSpace(payload?.Message))
                    {
                        message = payload.Message;
                    }
                }
                catch
                {
                    // Keep default message.
                }

                throw new InvalidOperationException(message);
            }
        }

        private sealed class ApiErrorPayload
        {
            public string? Message { get; set; }
        }
    }
}
