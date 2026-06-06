// <copyright file="IRentalPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IRentalPaymentService
    {
        Task<ServiceResult<RentalCheckoutDTO>> GetCheckoutSummaryAsync(int rentalId, Guid renterAccountId);

        Task<ServiceResult> CompleteCardPaymentAsync(CompleteRentalCardPaymentDTO payment);
    }

    public class RentalPaymentService : ApiServiceBase, IRentalPaymentService
    {
        public RentalPaymentService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<RentalCheckoutDTO>> GetCheckoutSummaryAsync(int rentalId, Guid renterAccountId)
            => GetAsync<RentalCheckoutDTO>($"api/rentals/{rentalId}/checkout?accountId={renterAccountId}");

        public Task<ServiceResult> CompleteCardPaymentAsync(CompleteRentalCardPaymentDTO payment)
            => PostAsync("api/rentals/complete-card-payment", payment);
    }
}
