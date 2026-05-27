// <copyright file="MessageDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;

namespace BoardGames.Shared.DTO
{
    public record MessageDTO(
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

            return this.Type switch
            {
                MessageType.MessageText or MessageType.MessageSystem => this.Content.Length > maximumPreviewLength ? this.Content[..maximumPreviewLength] : this.Content,
                MessageType.MessageImage => "[Image]",
                MessageType.MessageRentalRequest => "[Rental Request]",
                MessageType.MessageCashAgreement => "[Cash Agreement]",
                _ => "[Attachment]",
            };
        }
    }
}
