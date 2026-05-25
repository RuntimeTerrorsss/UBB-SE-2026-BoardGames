// <copyright file="PaymentDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }

        public string? DateText { get; set; }

        public string? ProductName { get; set; }

        public string? ReceiverName { get; set; }

        public string? OtherPartyName { get; set; }

        public string? Role { get; set; }

        public string? Period { get; set; }

        public string? Status { get; set; }

        public int RentalId { get; set; }

        public bool HasPayment { get; set; }

        public DateTime SortDate { get; set; }

        /// <summary>
        /// Gets or sets numeric amount used strictly for service-level total calculations.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets formatted amount string for display.
        /// </summary>
        public string AmountText => $"{this.Amount:C}";

        public string? PaymentMethod { get; set; }

        public string? FilePath { get; set; }
    }
}
