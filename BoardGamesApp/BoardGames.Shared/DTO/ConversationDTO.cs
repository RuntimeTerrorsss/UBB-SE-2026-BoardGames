// <copyright file="ConversationDTO.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGames.Shared.DTO
{
    public class ConversationDTO
    {
        public ConversationDTO()
        {
            MessageList = new List<MessageDataTransferObject>();
            ParticipantUserIds = new List<int>();
            LastRead = new Dictionary<int, DateTime>();
            UnreadCount = new Dictionary<int, int>();
        }

        public ConversationDTO(
            int conversationId,
            IReadOnlyList<int> participantUserIds,
            List<MessageDataTransferObject> messages,
            Dictionary<int, DateTime> lastRead)
        {
            Id = conversationId;
            ParticipantUserIds = participantUserIds;
            MessageList = messages;
            LastRead = lastRead;
            UnreadCount = participantUserIds.ToDictionary(userId => userId, _ => 0);
            UpdateUnreadCounts();
        }

        public int Id { get; set; }

        public List<MessageDataTransferObject> MessageList { get; set; }

        public IReadOnlyList<int> ParticipantUserIds { get; set; }

        public Dictionary<int, DateTime> LastRead { get; set; }

        public Dictionary<int, int> UnreadCount { get; set; }

        public Dictionary<int, string> ParticipantDisplayNames { get; set; } = new();

        public void AddMessageToListDTO(MessageDataTransferObject newMessage)
        {
            if (MessageList.Any(message => message.Id == newMessage.Id))
            {
                return;
            }

            MessageList.Add(newMessage);
            UpdateUnreadCounts();
        }

        public void UpdateUnreadCounts()
        {
            int defaultUnreadCount = 0;
            int systemMessageSenderIdentifier = 0;

            foreach (var participantUserId in ParticipantUserIds)
            {
                UnreadCount[participantUserId] = defaultUnreadCount;
            }

            foreach (var messageItem in MessageList)
            {
                if (messageItem.ReceiverId == systemMessageSenderIdentifier)
                {
                    continue;
                }

                DateTime receiverLastRead = LastRead.TryGetValue(messageItem.ReceiverId, out DateTime readTime)
                    ? readTime
                    : DateTime.MinValue;

                if (messageItem.SentAt >= receiverLastRead
                    && UnreadCount.TryGetValue(messageItem.ReceiverId, out int _))
                {
                    UnreadCount[messageItem.ReceiverId]++;
                }
            }
        }
    }
}
