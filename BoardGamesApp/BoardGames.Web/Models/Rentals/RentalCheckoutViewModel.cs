// <copyright file="RentalCheckoutViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace BoardGames.Web.Models.Rentals
{
    public class RentalCheckoutViewModel
    {
        public int RentalId { get; set; }

        public int MessageId { get; set; }

        public int GameId { get; set; }

        public string GameName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public int ClientId { get; set; }

        public int OwnerId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal TotalPrice { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Required]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Street")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Street number")]
        public string StreetNumber { get; set; } = string.Empty;

        [Display(Name = "Save address to my profile")]
        public bool SaveAddress { get; set; }

        [Required]
        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; } = "Card";
    }
}
