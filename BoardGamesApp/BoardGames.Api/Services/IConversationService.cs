using BoardGames.Shared.DTO;
// <copyright file="IConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


namespace BoardGames.Api.Services
{
    public interface IConversationService
    {
        void Initialize(int userIdInput);
        void OnMessageReceived(Message message);

        void OnMessageUpdateReceived(Message message);

        Task OnConversationReceived(Conversation conversation);

        void OnReadReceiptReceived(ReadReceiptDTO readReceipt);

        Task<List<ConversationDTO>> FetchConversations();

        Task<string> GetOtherUserNameByConversationDTO(ConversationDTO conversation);

        Task UpdateMessage(MessageDataTransferObject message);

        Task SendMessage(MessageDataTransferObject message);

        Task OnCardPaymentSelected(int messageId);

        Task OnCashPaymentSelected(int messageId, int paymentId);

        Task<int> FindOrCreateConversationBetweenUsers(int userIdA, int userIdB);

        void StartPolling();

        void StopPolling();

        event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        event Action<ConversationDTO, string> ActionConversationProcessed;

        event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;
    }
}
