namespace BoardGames.Shared.DTO
{
    public record ReadReceiptDTO(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime ReceiptTimeStamp);
}
