// <copyright file="ConversationNotifier.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Sharing.DTO;

namespace BoardGames.Api.Services
{
    public class ConversationNotifier : IConversationNotifier
    {
        private readonly object subscribersLock = new object();
        private readonly Dictionary<int, IConversationService> subscribers = new Dictionary<int, IConversationService>();

        public void Register(int userId, IConversationService observer)
        {
            lock (subscribersLock)
            {
                subscribers[userId] = observer;
            }
        }

        public void Unregister(int userId)
        {
            lock (subscribersLock)
            {
                subscribers.Remove(userId);
            }
        }

        public void NotifyMessage(IEnumerable<int> participantUserIds, Message message)
        {
            foreach (IConversationService observer in SnapshotSubscribers(participantUserIds))
            {
                observer.OnMessageReceived(message);
            }
        }

        public void NotifyMessageUpdate(IEnumerable<int> participantUserIds, Message message)
        {
            foreach (IConversationService observer in SnapshotSubscribers(participantUserIds))
            {
                observer.OnMessageUpdateReceived(message);
            }
        }

        public void NotifyReadReceipt(IEnumerable<int> participantUserIds, ReadReceiptDTO readReceipt)
        {
            foreach (IConversationService observer in SnapshotSubscribers(participantUserIds))
            {
                observer.OnReadReceiptReceived(readReceipt);
            }
        }

        public void NotifyNewConversation(Conversation conversation)
        {
            IEnumerable<int> participantUserIds = conversation.Participants.Select(participant => participant.UserId);
            foreach (IConversationService observer in SnapshotSubscribers(participantUserIds))
            {
                observer.OnConversationReceived(conversation);
            }
        }

        private List<IConversationService> SnapshotSubscribers(IEnumerable<int> userIds)
        {
            var observers = new List<IConversationService>();
            var distinctIds = userIds.Distinct().ToList();

            lock (subscribersLock)
            {
                foreach (int userId in distinctIds)
                {
                    if (subscribers.TryGetValue(userId, out IConversationService? observer))
                    {
                        observers.Add(observer);
                    }
                }
            }

            return observers;
        }
    }
}
