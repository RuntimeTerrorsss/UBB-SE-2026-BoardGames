// <copyright file="ChatPageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Repositories;
using BookingBoardGames.Sharing.Services;

namespace BoardGames.Desktop.ViewModels;

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
        conversationService.Initialize(currentUser);
    }

    public ChatPageViewModel(int currentUser, ConversationService service)
        : this(currentUser, service, App.UserRepository)
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service, IUserRepository userRepository)
    {
        LeftPanelModelView = new LeftPanelViewModel();
        ChatModelView = new ChatViewModel(currentUser);
        currentUserId = currentUser;
        this.userRepository = userRepository;

        LeftPanelModelView.PropertyChanged += OnLeftPanelPropertyChanged;
        ChatModelView.MessageSent += OnMessageSent;
        ChatModelView.BookingRequestUpdate += UpdateBookingRequest;
        ChatModelView.CashAgreementAccept += UpdateCashAgreement;

        conversationService = service;

        conversationService.ActionMessageProcessed += this.OnMessageReceived;
        conversationService.ActionConversationProcessed += this.OnConversationReceived;
        conversationService.ActionReadReceiptProcessed += this.OnReadReceiptReceived;
        conversationService.ActionMessageUpdateProcessed += this.OnMessageUpdateReceived;
    }

    public string CurrentUsername
    {
        get => currentUsername;
        set => SetProperty(ref currentUsername, value);
    }

    public async Task InitializeAsync()
    {
        var fetchedConversations = await conversationService.FetchConversations();

        var user = await userRepository.GetById(currentUserId);
        CurrentUsername = user?.Username ?? string.Empty;

        if (currentUserId == MainWindow.loggedInUserAlice || currentUserId == MainWindow.loggedInUserBob)
        {
            int otherDemoUserId = currentUserId == MainWindow.loggedInUserAlice
                ? MainWindow.loggedInUserBob
                : MainWindow.loggedInUserAlice;

            bool hasConversationWithOtherDemoUser = fetchedConversations.Any(conversation =>
                conversation.Participants.Any(participant => participant.UserId == otherDemoUserId));

            if (!hasConversationWithOtherDemoUser)
            {
                await conversationService.CreateConversation(currentUserId, otherDemoUserId);
                fetchedConversations = await conversationService.FetchConversations();
            }
        }

        conversations.AddRange(fetchedConversations);

        foreach (var conversationItem in conversations)
        {
            await LeftPanelModelView.HandleIncomingConversation(
                conversationItem,
                await conversationService.GetOtherUserNameByConversationDTO(conversationItem),
                currentUserId,
                userRepository);
        }

        conversationService.StartPolling();
    }

    public LeftPanelViewModel LeftPanelModelView { get; }

    public ChatViewModel ChatModelView { get; }

    public ConversationService ConversationService
    {
        get => conversationService;
    }

    private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
        {
            return;
        }

        if (LeftPanelModelView.SelectedConversation == null)
        {
            return;
        }

        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == LeftPanelModelView.SelectedConversation.ConversationId);
        if (matchedConversation == null)
        {
            return;
        }

        int selectedConversationOtherUserUnreadCount = matchedConversation.UnreadCount.FirstOrDefault(unreadItem => unreadItem.Key != currentUserId).Value;
        ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, selectedConversationOtherUserUnreadCount);

        SendReadReceipt(matchedConversation);
    }

    private async void OnMessageSent(MessageDataTransferObject message)
    {
        try
        {
            var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);
            if (matchedConversation is null)
            {
                return;
            }

            int receiverUserId = matchedConversation.Participants.First(participantItem => participantItem.UserId != message.SenderId).UserId;
            message = message with { ReceiverId = receiverUserId };
            await conversationService.SendMessage(message);
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
            await conversationService.SendReadReceipt(conversation);
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
            await conversationService.UpdateMessage(message);
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
                var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);

                matchedConversation?.AddMessageToListDTO(message);

                int resolvedOtherUserId = message.SenderId == currentUserId ? message.ReceiverId : message.SenderId;
                if (resolvedOtherUserId <= 0 || resolvedOtherUserId == currentUserId)
                {
                    int participantDerivedUserId = matchedConversation?.Participants
                        .Where(participantItem => participantItem.UserId != currentUserId && participantItem.UserId != 1)
                        .Select(participantItem => participantItem.UserId)
                        .FirstOrDefault() ?? 0;

                    if (participantDerivedUserId <= 0)
                    {
                        var refreshedConversation = (await conversationService.FetchConversations())
                            .FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);
                        if (refreshedConversation is not null && matchedConversation is null)
                        {
                            conversations.Add(refreshedConversation);
                            matchedConversation = refreshedConversation;
                        }

                        participantDerivedUserId = refreshedConversation?.Participants
                            .Where(participantItem => participantItem.UserId != currentUserId && participantItem.UserId != 1)
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
                    var otherUser = await userRepository.GetById(resolvedOtherUserId);
                    senderName = otherUser?.Username ?? senderName;
                }

                if (matchedConversation is not null &&
                    string.Equals(senderName, "System", StringComparison.OrdinalIgnoreCase))
                {
                    int otherUserId = matchedConversation.Participants
                        .Where(participantItem => participantItem.UserId != currentUserId)
                        .Select(participantItem => participantItem.UserId)
                        .FirstOrDefault();

                    if (otherUserId > 0)
                    {
                        var otherUser = await userRepository.GetById(otherUserId);
                        if (otherUser is not null &&
                            !string.Equals(otherUser.Username, "System", StringComparison.OrdinalIgnoreCase))
                        {
                            senderName = otherUser.Username;
                        }
                    }
                }

                await LeftPanelModelView.HandleIncomingMessage(message, senderName);
                ChatModelView.HandleIncomingMessage(message);
                if (ChatModelView.ConversationId == message.ConversationId && matchedConversation is not null)
                {
                    SendReadReceipt(matchedConversation);
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
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
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
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        if (currentUserId == targetMessage.SenderId)
        {
            targetMessage = targetMessage with { IsAcceptedBySeller = true };
        }

        if (currentUserId == targetMessage.ReceiverId)
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
                conversations.Add(conversation);
                await LeftPanelModelView.HandleIncomingConversation(conversation, otherUsername, currentUserId);
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
                var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == readReceipt.ConversationId);
                if (matchedConversation != null)
                {
                    matchedConversation.LastRead[readReceipt.ReaderId] = readReceipt.ReceiptTimeStamp;
                    matchedConversation.UpdateUnreadCounts();
                    if (ChatModelView.ConversationId == readReceipt.ConversationId && readReceipt.ReaderId != currentUserId)
                    {
                        ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, matchedConversation.UnreadCount[readReceipt.ReaderId]);
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
                var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == updatedMessage.ConversationId);
                if (matchedConversation == null)
                {
                    return;
                }

                for (int messageIndex = 0; messageIndex < matchedConversation.MessageList.Count; messageIndex++)
                {
                    if (matchedConversation.MessageList[messageIndex].Id == updatedMessage.Id)
                    {
                        matchedConversation.MessageList[messageIndex] = updatedMessage;
                        if (ChatModelView.ConversationId == updatedMessage.ConversationId)
                        {
                            ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, noUnreadMessagesCount);
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
