// <copyright file="IReceiptService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Api.Services
{
    public interface IReceiptService
    {
        public string GenerateReceiptRelativePath(int rentalId);

        public Task<string> GetReceiptDocument(Payment payment);
    }
}
