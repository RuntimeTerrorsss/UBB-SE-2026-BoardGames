// <copyright file="ConversationParticipantDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class ConversationParticipantDTO
    {
        public int ConversationId { get; set; }

        public int UserId { get; set; }

        public DateTime? LastMessageReadTime { get; set; }

        public int UnreadMessagesCount { get; set; }
    }
}
