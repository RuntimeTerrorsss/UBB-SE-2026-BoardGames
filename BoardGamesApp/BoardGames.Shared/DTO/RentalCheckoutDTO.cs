// <copyright file="RentalCheckoutDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public sealed class RentalCheckoutDTO
    {
        public int RentalId { get; set; }

        public string GameName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string DateRange { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal Balance { get; set; }
    }

    public sealed class CompleteRentalCardPaymentDTO
    {
        public int RequestId { get; set; }

        public int RentalId { get; set; }

        public int MessageId { get; set; }

        public Guid RenterAccountId { get; set; }

        public string CardNumber { get; set; } = string.Empty;

        public string CardholderName { get; set; } = string.Empty;

        public string ExpiryDate { get; set; } = string.Empty;

        public string CardVerificationValue { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "CARD";
    }
}
