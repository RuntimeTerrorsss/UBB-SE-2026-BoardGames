using BoardGames.Shared.DTO;
// <copyright file="ICashPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


namespace BoardGames.Api.Services
{
    public interface ICashPaymentService
    {
        public Task<int> AddCashPaymentAsync(CashPaymentDataTransferObject paymentDto);

        public Task<CashPaymentDataTransferObject> GetCashPaymentAsync(int paymentId);

        public Task ConfirmDeliveryAsync(int paymentId);

        public Task ConfirmPaymentAsync(int paymentId);

        public Task<bool> IsAllConfirmedAsync(int paymentId);

        public Task<bool> IsDeliveryConfirmedAsync(int paymentId);

        public Task<bool> IsPaymentConfirmedAsync(int paymentId);
    }
}
