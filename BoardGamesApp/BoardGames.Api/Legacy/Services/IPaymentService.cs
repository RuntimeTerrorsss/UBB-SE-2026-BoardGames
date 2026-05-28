// <copyright file="IPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Api.Legacy.Services
{
    public interface IPaymentService
    {
        Task GenerateReceiptAsync(int paymentId);

        Task<string> GetReceiptAsync(int paymentId);
    }
}
