// <copyright file="IConversationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

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

        Task UpdateMessage(MessageDTO message);

        Task SendMessage(MessageDTO message);

        Task OnCardPaymentSelected(int messageId);

        Task OnCashPaymentSelected(int messageId, int paymentId);

        Task<int> FindOrCreateConversationBetweenUsers(int userIdA, int userIdB);

        void StartPolling();

        void StopPolling();

        event Action<MessageDTO, string> ActionMessageProcessed;

        event Action<ConversationDTO, string> ActionConversationProcessed;

        event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        event Action<MessageDTO, string> ActionMessageUpdateProcessed;
    }
}
