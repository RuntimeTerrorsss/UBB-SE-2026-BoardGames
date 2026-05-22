// <copyright file="MessageTypeExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Data.Enum
{
    public enum MessageType
    {
        MessageSystem,
        MessageText,
        MessageImage,
        MessageRentalRequest,
        MessageCashAgreement,
    }

    public static class MessageTypeExtensions
    {
        public static string MessageTypeToString(this MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.MessageText:
                    return "TEXT";
                case MessageType.MessageImage:
                    return "IMAGE";
                case MessageType.MessageRentalRequest:
                    return "RENTAL_REQUEST";
                case MessageType.MessageCashAgreement:
                    return "CASH_AGREEMENT";
                case MessageType.MessageSystem:
                    return "SYSTEM";
                default:
                    return "Unknown";
            }
        }
    }
}
