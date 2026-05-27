// <copyright file="CardPaymentDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class CardPaymentDTO(
        int transactionIdentifier,
        int requestIdentifier,
        int clientIdentifier,
        int ownerIdentifier,
        decimal amount,
        DateTime dateOfTransaction,
        string paymentMethod)
    {
        public int TransactionIdentifier { get; set; } = transactionIdentifier;

        public int RequestIdentifier { get; set; } = requestIdentifier;

        public int ClientIdentifier { get; set; } = clientIdentifier;

        public int OwnerIdentifier { get; set; } = ownerIdentifier;

        public decimal Amount { get; set; } = amount;

        public DateTime DateOfTransaction { get; set; } = dateOfTransaction;

        public string PaymentMethod { get; set; } = paymentMethod;
    }
}
