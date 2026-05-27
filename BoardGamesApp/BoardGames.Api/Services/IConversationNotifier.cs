// <copyright file="IConversationNotifier.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;

namespace BoardGames.Api.Services
{
    public interface IConversationNotifier
    {
        void Register(int userId, IConversationService observer);

        void Unregister(int userId);

        void NotifyMessage(IEnumerable<int> participantUserIds, Message message);

        void NotifyMessageUpdate(IEnumerable<int> participantUserIds, Message message);

        void NotifyReadReceipt(IEnumerable<int> participantUserIds, ReadReceiptDTO readReceipt);

        void NotifyNewConversation(Conversation conversation);
    }
}
