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
        public ConversationDTO(
            int conversationId,
            ICollection<ConversationParticipant> participants,
            List<MessageDataTransferObject> messages,
            Dictionary<int, DateTime> lastRead)
        {
            Id = conversationId;
            Participants = participants;
            MessageList = messages;
            LastRead = lastRead;
            UnreadCount = participants.ToDictionary(participant => participant.UserId, _ => 0);
            UpdateUnreadCounts();
        }

        public int Id { get; set; }

        public List<MessageDataTransferObject> MessageList { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; }

        public Dictionary<int, DateTime> LastRead { get; set; }

        public Dictionary<int, int> UnreadCount { get; set; }

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

            foreach (var participantItem in Participants)
            {
                UnreadCount[participantItem.UserId] = defaultUnreadCount;
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
