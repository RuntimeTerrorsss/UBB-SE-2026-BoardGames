// <copyright file="IConversationRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IConversationRepository
    {
        Task<List<Conversation>> GetConversationsForUser(int userId);

        Task<Conversation> GetConversationById(int conversationId);

        Task<IReadOnlyList<int>> GetParticipantUserIds(int conversationId);

        Task<Message> HandleNewMessage(Message message);

        Task<Message?> HandleMessageUpdate(Message message);

        Task HandleReadReceipt(ReadReceiptDTO readReceipt);

        Task<int> CreateConversation(int senderId, int receiverId);

        Task<int> FindOrCreateConversationBetweenUsers(int userIdA, int userIdB);

        Task<Message?> HandleRentalRequestFinalization(int messageId);

        Task<Message?> CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId);

        Task<RentalRequestMessage?> FindRentalRequestMessageByRequestId(int requestId);

        Task<RentalRequestMessage?> AcceptRentalRequestByRequestId(int requestId, int rentalId);

        Task<RentalRequestMessage?> GetRentalRequestMessageById(int messageId);

        Task FinalizeRentalRequestByMessageId(int messageId, bool accepted);
    }
}
