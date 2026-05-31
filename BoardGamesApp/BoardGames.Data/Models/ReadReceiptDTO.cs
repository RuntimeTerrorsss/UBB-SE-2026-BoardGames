using System;

namespace BoardGames.Data.Models
{
    public record ReadReceiptDTO(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime ReceiptTimeStamp);
}
