// <copyright file="LeftPanelViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using Microsoft.UI.Xaml;

namespace BookingBoardGames.Src.ViewModels
{
    public class LeftPanelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEmptyStateVisible => this.allConversations.Count == 0;

        public bool IsNoMatchesVisible => this.allConversations.Count > 0 && this.Conversations.Count == 0;

        public bool IsListVisible => this.Conversations.Count > 0;

        private void RefreshUIStates()
        {
            this.OnPropertyChanged(nameof(this.IsEmptyStateVisible));
            this.OnPropertyChanged(nameof(this.IsNoMatchesVisible));
            this.OnPropertyChanged(nameof(this.IsListVisible));
        }

        private List<ConversationPreviewModel> allConversations = new();

        private ObservableCollection<ConversationPreviewModel> conversations;

        public ObservableCollection<ConversationPreviewModel> Conversations
        {
            get => this.conversations;
            set
            {
                this.conversations = value;
                this.OnPropertyChanged();
            }
        }

        private string searchText = string.Empty;

        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;
                    this.OnPropertyChanged();
                    this.ApplyFilter();
                }
            }
        }

        private int? selectedConversationId;

        public ConversationPreviewModel SelectedConversation
        {
            get => this.Conversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == this.selectedConversationId);
            set
            {
                if (this.selectedConversationId != value?.ConversationId)
                {
                    this.selectedConversationId = value?.ConversationId;

                    if (this.selectedConversationId.HasValue)
                    {
                        this.MarkAsRead(this.selectedConversationId.Value);
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        public LeftPanelViewModel()
        {
            this.Conversations = new ObservableCollection<ConversationPreviewModel>();
        }

        private void ApplyFilter()
        {
            var filteredConversations = this.allConversations
                .Where(conversationItem => string.IsNullOrEmpty(this.SearchText) ||
                            conversationItem.DisplayName.Contains(this.SearchText, StringComparison.Ordinal))
                .ToList();

            for (int conversationIndex = this.Conversations.Count - 1; conversationIndex >= 0; conversationIndex--)
            {
                if (!filteredConversations.Contains(this.Conversations[conversationIndex]))
                {
                    this.Conversations.RemoveAt(conversationIndex);
                }
            }

            int notFoundIndex = -1;

            for (int filteredConIndex = 0; filteredConIndex < filteredConversations.Count; filteredConIndex++)
            {
                var filterItem = filteredConversations[filteredConIndex];
                int currentIndex = this.Conversations.IndexOf(filterItem);

                if (currentIndex == notFoundIndex)
                {
                    this.Conversations.Insert(filteredConIndex, filterItem);
                }
                else if (currentIndex != filteredConIndex)
                {
                    this.Conversations.Move(currentIndex, filteredConIndex);
                }
            }

            this.OnPropertyChanged(nameof(this.SelectedConversation));
            this.RefreshUIStates();
        }

        private void MarkAsRead(int conversationId)
        {
            int noUnreadMessagesCount = 0;
            var matchedConversation = this.allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversationId);
            if (matchedConversation == null || matchedConversation.UnreadCount == noUnreadMessagesCount)
            {
                return;
            }

            matchedConversation.UnreadCount = noUnreadMessagesCount;
        }

        public async Task HandleIncomingMessage(MessageDataTransferObject message, string senderName)
        {
            await this.HandleIncomingMessage(message, senderName, App.UserRepository);
        }

        public async Task HandleIncomingMessage(MessageDataTransferObject message, string senderName, IUserRepository userService)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;
            int noUnreadMessagesCount = 0;
            int singleUnreadMessageCount = 1;

            var matchedConversation = this.allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == message.ConversationId);

            if (matchedConversation != null)
            {
                matchedConversation.LastMessageText = message.Content;
                matchedConversation.Timestamp = DateTime.Now;
                matchedConversation.UnreadCount = message.ConversationId == this.selectedConversationId ? noUnreadMessagesCount : matchedConversation.UnreadCount + singleUnreadMessageCount;

                this.allConversations.Remove(matchedConversation);
                this.allConversations.Insert(0, matchedConversation);
            }
            else
            {
                var receiverUser = await userService.GetById(message.ReceiverId);
                var newConversationPreview = new ConversationPreviewModel(
                    message.ConversationId,
                    senderName,
                    senderName.Substring(firstCharacterIndex, singleCharacterLength).ToUpper(),
                    message.Content,
                    DateTime.Now,
                    unreadCountInput: message.ConversationId == this.selectedConversationId ? noUnreadMessagesCount : singleUnreadMessageCount,
                    receiverUser?.AvatarUrl ?? string.Empty);
                this.allConversations.Insert(0, newConversationPreview);
            }

            this.ApplyFilter();
        }

        public Task HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId)
        {
            return this.HandleIncomingConversation(conversation, displayName, userId, App.UserRepository);
        }

        public async Task HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId, IUserRepository service)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;

            var matchedConversation = this.allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversation.Id);
            if (matchedConversation != null)
            {
                return;
            }

            var otherParticipantIds = conversation.Participants
                .Select(participantItem => participantItem.UserId)
                .Where(participantId => participantId != userId)
                .Distinct()
                .ToList();

            if (otherParticipantIds.Count == 0)
            {
                return;
            }

            int otherUserIdentifier = otherParticipantIds.First();
            foreach (var participantId in otherParticipantIds)
            {
                var candidate = await service.GetById(participantId);
                if (candidate is not null &&
                    !string.Equals(candidate.Username, "System", StringComparison.OrdinalIgnoreCase))
                {
                    otherUserIdentifier = participantId;
                    break;
                }
            }

            var otherUser = await service.GetById(otherUserIdentifier);
            int unreadCount = conversation.UnreadCount.TryGetValue(userId, out var count) ? count : 0;
            string safeDisplayName = string.IsNullOrWhiteSpace(displayName) ? "Unknown User" : displayName;
            string initials = safeDisplayName.Substring(firstCharacterIndex, singleCharacterLength).ToUpper();

            var newConversationPreview = new ConversationPreviewModel(
                conversation.Id,
                safeDisplayName,
                initials,
                conversation.MessageList.LastOrDefault()?.GetChatMessagePreview() ?? string.Empty,
                conversation.MessageList.LastOrDefault()?.SentAt ?? DateTime.MinValue,
                unreadCountInput: unreadCount,
                otherUser?.AvatarUrl ?? string.Empty);

            this.allConversations.Insert(0, newConversationPreview);
            this.SortConversationsByTimestamp();
            this.ApplyFilter();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SortConversationsByTimestamp()
        {
            this.allConversations = this.allConversations.OrderByDescending(conversationItem => conversationItem.Timestamp).ToList();
            Debug.WriteLine("sorted conversations:");
            this.ApplyFilter();
        }

        public void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
