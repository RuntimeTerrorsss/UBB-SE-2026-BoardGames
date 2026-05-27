// <copyright file="LeftPanelViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.ViewModels
{
    public class LeftPanelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEmptyStateVisible => allConversations.Count == 0;

        public bool IsNoMatchesVisible => allConversations.Count > 0 && Conversations.Count == 0;

        public bool IsListVisible => Conversations.Count > 0;

        private void RefreshUIStates()
        {
            OnPropertyChanged(nameof(IsEmptyStateVisible));
            OnPropertyChanged(nameof(IsNoMatchesVisible));
            OnPropertyChanged(nameof(IsListVisible));
        }

        private List<ConversationPreviewModel> allConversations = new();

        private ObservableCollection<ConversationPreviewModel> conversations;

        public ObservableCollection<ConversationPreviewModel> Conversations
        {
            get => conversations;
            set
            {
                conversations = value;
                OnPropertyChanged();
            }
        }

        private string searchText = string.Empty;

        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        private int? selectedConversationId;

        public ConversationPreviewModel SelectedConversation
        {
            get => Conversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == selectedConversationId);
            set
            {
                if (selectedConversationId != value?.ConversationId)
                {
                    selectedConversationId = value?.ConversationId;

                    if (selectedConversationId.HasValue)
                    {
                        MarkAsRead(selectedConversationId.Value);
                    }

                    OnPropertyChanged();
                }
            }
        }

        public LeftPanelViewModel()
        {
            Conversations = new ObservableCollection<ConversationPreviewModel>();
        }

        private void ApplyFilter()
        {
            var filteredConversations = allConversations
                .Where(conversationItem => string.IsNullOrEmpty(SearchText) ||
                            conversationItem.DisplayName.Contains(SearchText, StringComparison.Ordinal))
                .ToList();

            for (int conversationIndex = Conversations.Count - 1; conversationIndex >= 0; conversationIndex--)
            {
                if (!filteredConversations.Contains(Conversations[conversationIndex]))
                {
                    Conversations.RemoveAt(conversationIndex);
                }
            }

            int notFoundIndex = -1;

            for (int filteredConIndex = 0; filteredConIndex < filteredConversations.Count; filteredConIndex++)
            {
                var filterItem = filteredConversations[filteredConIndex];
                int currentIndex = Conversations.IndexOf(filterItem);

                if (currentIndex == notFoundIndex)
                {
                    Conversations.Insert(filteredConIndex, filterItem);
                }
                else if (currentIndex != filteredConIndex)
                {
                    Conversations.Move(currentIndex, filteredConIndex);
                }
            }

            OnPropertyChanged(nameof(SelectedConversation));
            RefreshUIStates();
        }

        private void MarkAsRead(int conversationId)
        {
            int noUnreadMessagesCount = 0;
            var matchedConversation = allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversationId);
            if (matchedConversation == null || matchedConversation.UnreadCount == noUnreadMessagesCount)
            {
                return;
            }

            matchedConversation.UnreadCount = noUnreadMessagesCount;
        }

        public async Task HandleIncomingMessage(MessageDTO message, string senderName)
        {
            await this.HandleIncomingMessage(message, senderName, App.UserRepository);
        }

        public async Task HandleIncomingMessage(MessageDTO message, string senderName, IUserRepository userService)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;
            int noUnreadMessagesCount = 0;
            int singleUnreadMessageCount = 1;

            var matchedConversation = allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == message.ConversationId);

            if (matchedConversation != null)
            {
                matchedConversation.LastMessageText = message.Content;
                matchedConversation.Timestamp = DateTime.Now;
                matchedConversation.UnreadCount = message.ConversationId == selectedConversationId ? noUnreadMessagesCount : matchedConversation.UnreadCount + singleUnreadMessageCount;

                allConversations.Remove(matchedConversation);
                allConversations.Insert(0, matchedConversation);
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
                    unreadCountInput: message.ConversationId == selectedConversationId ? noUnreadMessagesCount : singleUnreadMessageCount,
                    receiverUser?.AvatarUrl ?? string.Empty);
                allConversations.Insert(0, newConversationPreview);
            }

            ApplyFilter();
        }

        public Task HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId)
        {
            return this.HandleIncomingConversation(conversation, displayName, userId, App.UserRepository);
        }

        public async Task HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId, IUserRepository service)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;

            var matchedConversation = allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversation.Id);
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

            allConversations.Insert(0, newConversationPreview);
            SortConversationsByTimestamp();
            ApplyFilter();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SortConversationsByTimestamp()
        {
            allConversations = allConversations.OrderByDescending(conversationItem => conversationItem.Timestamp).ToList();
            Debug.WriteLine("sorted conversations:");
            ApplyFilter();
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
