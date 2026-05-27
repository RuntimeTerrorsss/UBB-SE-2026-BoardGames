using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IConversationApiService
    {
        Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId);

        Task<ConversationDTO?> GetConversationById(int conversationId);

        Task<MessageDTO> SendMessage(MessageDTO dto);

        Task<MessageDTO?> UpdateMessage(MessageDTO dto);

        Task HandleReadReceipt(ReadReceiptDTO dto);

        Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB);

        Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end);

        Task FinalizeRentalRequestMessage(int requestId, bool accepted);

        Task<MessageDTO?> CreateCashAgreementMessage(int parentMessageId, int paymentId);
    }
}
