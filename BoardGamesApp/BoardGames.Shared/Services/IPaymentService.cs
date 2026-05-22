// <copyright file="IPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace BoardGames.Shared.Services
{
    public interface IPaymentService
    {
        Task GenerateReceiptAsync(int paymentId);

        Task<string> GetReceiptAsync(int paymentId);
    }
}
