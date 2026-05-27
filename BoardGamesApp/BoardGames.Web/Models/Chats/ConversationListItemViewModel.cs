// <copyright file="ConversationListItemViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Models.Chats
{
    public class ConversationListItemViewModel
    {
        public int ConversationId { get; set; }

        public string OtherUserName { get; set; } = string.Empty;

        public string? OtherUserAvatarUrl { get; set; }

        public string LastMessagePreview { get; set; } = string.Empty;
    }
}
