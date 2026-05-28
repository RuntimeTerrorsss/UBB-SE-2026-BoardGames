namespace BoardGames.Shared.ProxyServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardGames.Shared.DTO;

    public interface IConversationService
    {
        Task<ServiceResult<List<ConversationDTO>>> GetConversationsForUserAsync(Guid accountId);

        Task<ServiceResult<ConversationDTO>> GetConversationByIdAsync(int id);

        Task<ServiceResult<MessageDataTransferObject>> SendMessageAsync(MessageDataTransferObject messageDto);

        Task<ServiceResult<MessageDataTransferObject>> UpdateMessageAsync(MessageDataTransferObject messageDto);

        Task<ServiceResult> SendReadReceiptAsync(ReadReceiptDTO dto);

        Task<ServiceResult<ConversationDTO>> FindOrCreateConversationAsync(Guid senderId, Guid receiverId);
    }
}
