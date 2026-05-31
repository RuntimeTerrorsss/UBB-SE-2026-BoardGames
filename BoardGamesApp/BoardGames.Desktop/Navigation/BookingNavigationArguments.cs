// <copyright file="BookingNavigationArguments.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BoardGames.Api.Services;
using Microsoft.UI.Xaml;

namespace BoardGames.Desktop.Navigation
{
    public class BookingNavigationArguments
    {
        public int RequestIdentifier { get; set; }

        public required string DeliveryAddress { get; set; }

        public int BookingMessageIdentifier { get; set; }

        public required ConversationService ConversationService { get; set; }

        public required Window CurrentWindow { get; set; }
    }
}
