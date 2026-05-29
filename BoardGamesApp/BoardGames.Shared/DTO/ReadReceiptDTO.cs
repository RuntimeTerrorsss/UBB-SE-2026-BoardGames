// <copyright file="ReadReceiptDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public record ReadReceiptDTO(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime ReceiptTimeStamp);
}
