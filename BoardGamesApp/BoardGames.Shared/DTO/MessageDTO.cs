// <copyright file="MessageDataTransferObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace BoardGames.Shared.DTO
{
    public record MessageDataTransferObject(
        int Id,
        int ConversationId,
        int SenderId,
        int ReceiverId,
        DateTime SentAt,
        string Content,
        MessageType Type,
        string ImageUrl,
        bool IsResolved,
        bool IsAccepted,
        bool IsAcceptedByBuyer,
        bool IsAcceptedBySeller,
        int RequestId,
        int PaymentId)
    {
        public string GetChatMessagePreview()
        {
            int maximumPreviewLength = 50;

            return Type switch
            {
                MessageType.MessageText or MessageType.MessageSystem => Content.Length > maximumPreviewLength ? Content[..maximumPreviewLength] : Content,
                MessageType.MessageImage => "[Image]",
                MessageType.MessageRentalRequest => "[Rental Request]",
                MessageType.MessageCashAgreement => "[Cash Agreement]",
                _ => "[Attachment]",
            };
        }
    }
}
