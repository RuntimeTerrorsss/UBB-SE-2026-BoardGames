// <copyright file="CashPaymentDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class CashPaymentDTO(int paymentId, int requestId, int clientId, int ownerId, decimal amount)
    {
        public int Id { get; set; } = paymentId;

        public int RequestId { get; set; } = requestId;

        public int ClientId { get; set; } = clientId;

        public int OwnerId { get; set; } = ownerId;

        public decimal PaidAmount { get; set; } = amount;
    }
}
