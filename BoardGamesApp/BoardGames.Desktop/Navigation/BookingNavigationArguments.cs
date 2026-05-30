// <copyright file="BookingNavigationArguments.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using Microsoft.UI.Xaml;

namespace BoardGames.Desktop.Navigation
{
    public class BookingNavigationArguments
    {
        public int RentalId { get; set; }

        public int ChatRequestId { get; set; }

        public int MessageId { get; set; }

        public RentalCheckoutDTO Checkout { get; set; } = null!;

        public required string DeliveryAddress { get; set; }

        public required Window CurrentWindow { get; set; }
    }

    public sealed class DeliveryNavigationArgs
    {
        public RentalCheckoutDTO Checkout { get; init; } = null!;

        public int ChatRequestId { get; init; }

        public int MessageId { get; init; }

        public Window HostWindow { get; init; } = null!;
    }
}
