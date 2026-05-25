// <copyright file="ChatPageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;
using BookingBoardGames;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Repositories;
using BookingBoardGames.Sharing.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGames.Desktop.ViewModels
{

    public class ChatPageViewModel : ViewModelBase
    {
        private readonly int currentUserId;
        private readonly ConversationService conversationService;
        private readonly IUserRepository userRepository;
        private readonly List<ConversationDTO> conversations = new();
        private string currentUsername = string.Empty;

        public ChatPageViewModel(int currentUser)
            : this(currentUser, new ConversationService(App.ConversationRepository, App.UserRepository, App.ConversationNotifier))
        {
            this.conversationService.Initialize(currentUser);
        }

        public ChatPageViewModel(int currentUser, ConversationService service)
            : this(currentUser, service, App.UserRepository)
        {
        }

        public ChatPageViewModel(int currentUser, ConversationService service, IUserRepository userRepository)
        {
            this.LeftPanelModelView = new LeftPanelViewModel();
            this.ChatModelView = new ChatViewModel(currentUser);
            this.currentUserId = currentUser;
            this.userRepository = userRepository;

            this.LeftPanelModelView.PropertyChanged += this.OnLeftPanelPropertyChanged;
            this.ChatModelView.MessageSent += this.OnMessageSent;
            this.ChatModelView.BookingRequestUpdate += this.UpdateBookingRequest;
            this.ChatModelView.CashAgreementAccept += this.UpdateCashAgreement;

            this.conversationService = service;

            this.conversationService.ActionMessageProcessed += this.OnMessageReceived;
            this.conversationService.ActionConversationProcessed += this.OnConversationReceived;
            this.conversationService.ActionReadReceiptProcessed += this.OnReadReceiptReceived;
            this.conversationService.ActionMessageUpdateProcessed += this.OnMessageUpdateReceived;
        }

        public string CurrentUsername
        {
            get => this.currentUsername;
            set => this.SetProperty(ref this.currentUsername, value);
        }

        public async Task InitializeAsync()
        {
            var fetchedConversations = await this.conversationService.FetchConversations();

            var user = await this.userRepository.GetById(this.currentUserId);
            this.CurrentUsername = user?.Username ?? string.Empty;

            if (this.currentUserId == MainWindow.loggedInUserAlice || this.currentUserId == MainWindow.loggedInUserBob)
            {
                int otherDemoUserId = this.currentUserId == MainWindow.loggedInUserAlice
                    ? MainWindow.loggedInUserBob
                    : MainWindow.loggedInUserAlice;

                bool hasConversationWithOtherDemoUser = fetchedConversations.Any(conversation =>
                    conversation.Participants.Any(participant => participant.UserId == otherDemoUserId));

                if (!hasConversationWithOtherDemoUser)
                {
                    await this.conversationService.CreateConversation(this.currentUserId, otherDemoUserId);
                    fetchedConversations = await this.conversationService.FetchConversations();
                }
            }

            this.conversations.AddRange(fetchedConversations);

            foreach (var conversationItem in this.conversations)
            {
                await this.LeftPanelModelView.HandleIncomingConversation(
                    conversationItem,
                    await this.conversationService.GetOtherUserNameByConversationDTO(conversationItem),
                    this.currentUserId,
                    this.userRepository);
            }

            this.conversationService.StartPolling();
        }

        public LeftPanelViewModel LeftPanelModelView { get; }

        public ChatViewModel ChatModelView { get; }

        public ConversationService ConversationService
        {
            get => this.conversationService;
        }

        private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
            {
                return;
            }

            if (this.LeftPanelModelView.SelectedConversation == null)
            {
                return;
            }

            var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == this.LeftPanelModelView.SelectedConversation.ConversationId);
            if (matchedConversation == null)
            {
                return;
            }

            int selectedConversationOtherUserUnreadCount = matchedConversation.UnreadCount.FirstOrDefault(unreadItem => unreadItem.Key != this.currentUserId).Value;
            this.ChatModelView.LoadConversation(this.LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, selectedConversationOtherUserUnreadCount);

            this.SendReadReceipt(matchedConversation);
        }

        private async void OnMessageSent(MessageDataTransferObject message)
        {
            try
            {
                var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);
                if (matchedConversation is null)
                {
                    return;
                }

                int receiverUserId = matchedConversation.Participants.First(participantItem => participantItem.UserId != message.SenderId).UserId;
                message = message with { ReceiverId = receiverUserId };
                await this.conversationService.SendMessage(message);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"OnMessageSent failed: {exception.Message}");
            }
        }

        private async void SendReadReceipt(ConversationDTO conversation)
        {
            try
            {
                await this.conversationService.SendReadReceipt(conversation);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"SendReadReceipt failed: {exception.Message}");
            }
        }

        private async void OnSendMessageUpdate(MessageDataTransferObject message)
        {
            try
            {
                await this.conversationService.UpdateMessage(message);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"OnSendMessageUpdate failed: {exception.Message}");
            }
        }

        private void OnMessageReceived(MessageDataTransferObject message, string senderName)
        {
            ((App)Microsoft.UI.Xaml.Application.Current).Window?.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);

                    matchedConversation?.AddMessageToListDTO(message);

                    int resolvedOtherUserId = message.SenderId == this.currentUserId ? message.ReceiverId : message.SenderId;
                    if (resolvedOtherUserId <= 0 || resolvedOtherUserId == this.currentUserId)
                    {
                        int participantDerivedUserId = matchedConversation?.Participants
                            .Where(participantItem => participantItem.UserId != this.currentUserId && participantItem.UserId != 1)
                            .Select(participantItem => participantItem.UserId)
                            .FirstOrDefault() ?? 0;

                        if (participantDerivedUserId <= 0)
                        {
                            var refreshedConversation = (await this.conversationService.FetchConversations())
                                .FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);
                            if (refreshedConversation is not null && matchedConversation is null)
                            {
                                this.conversations.Add(refreshedConversation);
                                matchedConversation = refreshedConversation;
                            }

                            participantDerivedUserId = refreshedConversation?.Participants
                                .Where(participantItem => participantItem.UserId != this.currentUserId && participantItem.UserId != 1)
                                .Select(participantItem => participantItem.UserId)
                                .FirstOrDefault() ?? participantDerivedUserId;
                        }

                        if (participantDerivedUserId > 0)
                        {
                            resolvedOtherUserId = participantDerivedUserId;
                        }
                    }

                    if (senderName.StartsWith("User ", StringComparison.Ordinal) || resolvedOtherUserId > 0)
                    {
                        var otherUser = await this.userRepository.GetById(resolvedOtherUserId);
                        senderName = otherUser?.Username ?? senderName;
                    }

                    if (matchedConversation is not null &&
                        string.Equals(senderName, "System", StringComparison.OrdinalIgnoreCase))
                    {
                        int otherUserId = matchedConversation.Participants
                            .Where(participantItem => participantItem.UserId != this.currentUserId)
                            .Select(participantItem => participantItem.UserId)
                            .FirstOrDefault();

                        if (otherUserId > 0)
                        {
                            var otherUser = await this.userRepository.GetById(otherUserId);
                            if (otherUser is not null &&
                                !string.Equals(otherUser.Username, "System", StringComparison.OrdinalIgnoreCase))
                            {
                                senderName = otherUser.Username;
                            }
                        }
                    }

                    await this.LeftPanelModelView.HandleIncomingMessage(message, senderName);
                    this.ChatModelView.HandleIncomingMessage(message);
                    if (this.ChatModelView.ConversationId == message.ConversationId && matchedConversation is not null)
                    {
                        this.SendReadReceipt(matchedConversation);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"OnMessageReceived failed: {exception.Message}");
                }
            });
        }

        private void UpdateBookingRequest(int messageId, int conversationId, bool accepted, bool resolved)
        {
            var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
            var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
            if (targetMessage == null)
            {
                return;
            }

            targetMessage = targetMessage with { IsResolved = resolved, IsAccepted = accepted };
            this.OnSendMessageUpdate(targetMessage);
        }

        private void UpdateCashAgreement(int messageId, int conversationId)
        {
            var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
            var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
            if (targetMessage == null)
            {
                return;
            }

            if (this.currentUserId == targetMessage.SenderId)
            {
                targetMessage = targetMessage with { IsAcceptedBySeller = true };
            }

            if (this.currentUserId == targetMessage.ReceiverId)
            {
                targetMessage = targetMessage with { IsAcceptedByBuyer = true };
            }

            this.OnSendMessageUpdate(targetMessage);
        }

        private void OnConversationReceived(ConversationDTO conversation, string otherUsername)
        {
            ((App)Microsoft.UI.Xaml.Application.Current).Window?.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    this.conversations.Add(conversation);
                    await this.LeftPanelModelView.HandleIncomingConversation(conversation, otherUsername, this.currentUserId);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"OnConversationReceived failed: {exception.Message}");
                }
            });
        }

        private void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
        {
            ((App)Microsoft.UI.Xaml.Application.Current).Window?.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == readReceipt.ConversationId);
                    if (matchedConversation != null)
                    {
                        matchedConversation.LastRead[readReceipt.ReaderId] = readReceipt.ReceiptTimeStamp;
                        matchedConversation.UpdateUnreadCounts();
                        if (this.ChatModelView.ConversationId == readReceipt.ConversationId && readReceipt.ReaderId != this.currentUserId)
                        {
                            this.ChatModelView.LoadConversation(this.LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, matchedConversation.UnreadCount[readReceipt.ReaderId]);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"OnReadReceiptReceived failed: {exception.Message}");
                }
            });
        }

        private void OnMessageUpdateReceived(MessageDataTransferObject updatedMessage, string senderName)
        {
            ((App)Microsoft.UI.Xaml.Application.Current).Window?.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    int noUnreadMessagesCount = 0;
                    var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == updatedMessage.ConversationId);
                    if (matchedConversation == null)
                    {
                        return;
                    }

                    for (int messageIndex = 0; messageIndex < matchedConversation.MessageList.Count; messageIndex++)
                    {
                        if (matchedConversation.MessageList[messageIndex].Id == updatedMessage.Id)
                        {
                            matchedConversation.MessageList[messageIndex] = updatedMessage;
                            if (this.ChatModelView.ConversationId == updatedMessage.ConversationId)
                            {
                                this.ChatModelView.LoadConversation(this.LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, noUnreadMessagesCount);
                            }

                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"OnMessageUpdateReceived failed: {exception.Message}");
                }
            });
        }
    }
}
