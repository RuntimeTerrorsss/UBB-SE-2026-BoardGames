// <copyright file="IConversationNotifier.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Sharing.DTO;

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
