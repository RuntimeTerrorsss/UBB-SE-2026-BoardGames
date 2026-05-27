// <copyright file="ICashPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface ICashPaymentService
    {
        public Task<int> AddCashPaymentAsync(CashPaymentDTO paymentDto);

        public Task<CashPaymentDTO> GetCashPaymentAsync(int paymentId);

        public Task ConfirmDeliveryAsync(int paymentId);

        public Task ConfirmPaymentAsync(int paymentId);

        public Task<bool> IsAllConfirmedAsync(int paymentId);

        public Task<bool> IsDeliveryConfirmedAsync(int paymentId);

        public Task<bool> IsPaymentConfirmedAsync(int paymentId);
    }
}
