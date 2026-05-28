namespace BoardGames.Shared.ProxyServices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BoardGames.Data.Models;
    using BoardGames.Shared.DTO;

    public class ConversationService : ApiServiceBase, IConversationService
    {
        public ConversationService(HttpClient httpClient)
            : base((IHttpClientFactory)httpClient) { }

        public async Task<ServiceResult<List<ConversationDTO>>> GetConversationsForUserAsync(Guid accountId)
            => await GetAsync<List<ConversationDTO>>($"api/Conversation/user/{accountId}");

        public async Task<ServiceResult<ConversationDTO>> GetConversationByIdAsync(int id)
            => await GetAsync<ConversationDTO>($"api/Conversation/{id}");

        public async Task<ServiceResult<MessageDataTransferObject>> SendMessageAsync(MessageDataTransferObject messageDto)
            => await PostAsync<MessageDataTransferObject>("api/Conversation/messages", messageDto);

        public async Task<ServiceResult<MessageDataTransferObject>> UpdateMessageAsync(MessageDataTransferObject messageDto)
            => await PutAsync<MessageDataTransferObject>("api/Conversation/messages", messageDto);

        public async Task<ServiceResult> SendReadReceiptAsync(ReadReceiptDTO dto)
            => await PostAsync("api/Conversation/readreceipt", dto);

        public async Task<ServiceResult<ConversationDTO>> FindOrCreateConversationAsync(Guid senderId, Guid receiverId)
            => await PostAsync<ConversationDTO>("api/Conversation", new { SenderAccountId = senderId, ReceiverAccountId = receiverId });
    }
}
