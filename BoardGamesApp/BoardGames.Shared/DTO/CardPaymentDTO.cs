// <copyright file="CardPaymentDTO.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

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
