// <copyright file="ReadReceiptDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Data.Models
{
    public record ReadReceiptDTO(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime ReceiptTimeStamp);
}
