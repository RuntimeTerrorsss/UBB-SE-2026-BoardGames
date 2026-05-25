// <copyright file="IPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Api.Services
{
    public interface IPaymentService
    {
        Task GenerateReceiptAsync(int paymentId);

        Task<string> GetReceiptAsync(int paymentId);
    }
}
