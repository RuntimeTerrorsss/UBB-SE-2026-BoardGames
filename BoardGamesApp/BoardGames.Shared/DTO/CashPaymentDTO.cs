// <copyright file="CashPaymentDataTransferObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class CashPaymentDataTransferObject(int paymentId, int requestId, int clientId, int ownerId, decimal amount)
    {
        public int Id { get; set; } = paymentId;

        public int RequestId { get; set; } = requestId;

        public int ClientId { get; set; } = clientId;

        public int OwnerId { get; set; } = ownerId;

        public decimal PaidAmount { get; set; } = amount;
    }
}
