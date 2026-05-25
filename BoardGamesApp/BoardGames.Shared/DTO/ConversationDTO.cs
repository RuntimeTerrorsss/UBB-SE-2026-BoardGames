// <copyright file="ConversationDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class ConversationDTO
    {
        public ConversationDTO(
            int conversationId,
            ICollection<ConversationParticipant> participants,
            List<MessageDTO> messages,
            Dictionary<int, DateTime> lastRead)
        {
            this.Id = conversationId;
            this.Participants = participants;
            this.MessageList = messages;
            this.LastRead = lastRead;
            this.UnreadCount = participants.ToDictionary(participant => participant.UserId, _ => 0);
            this.UpdateUnreadCounts();
        }

        public int Id { get; set; }

        public List<MessageDTO> MessageList { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; }

        public Dictionary<int, DateTime> LastRead { get; set; }

        public Dictionary<int, int> UnreadCount { get; set; }

        public void AddMessageToListDTO(MessageDTO newMessage)
        {
            if (this.MessageList.Any(message => message.Id == newMessage.Id))
            {
                return;
            }

            this.MessageList.Add(newMessage);
            this.UpdateUnreadCounts();
        }

        public void UpdateUnreadCounts()
        {
            int defaultUnreadCount = 0;
            int systemMessageSenderIdentifier = 0;

            foreach (var participantItem in this.Participants)
            {
                this.UnreadCount[participantItem.UserId] = defaultUnreadCount;
            }

            foreach (var messageItem in this.MessageList)
            {
                if (messageItem.ReceiverId == systemMessageSenderIdentifier)
                {
                    continue;
                }

                DateTime receiverLastRead = this.LastRead.TryGetValue(messageItem.ReceiverId, out DateTime readTime)
                    ? readTime
                    : DateTime.MinValue;

                if (messageItem.SentAt >= receiverLastRead
                    && this.UnreadCount.TryGetValue(messageItem.ReceiverId, out int _))
                {
                    this.UnreadCount[messageItem.ReceiverId]++;
                }
            }
        }
    }
}
