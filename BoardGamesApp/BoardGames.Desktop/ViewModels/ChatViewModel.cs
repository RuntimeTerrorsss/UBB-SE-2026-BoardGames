// <copyright file="ChatViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;
using Microsoft.UI.Xaml.Controls;

namespace BoardGames.Desktop.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    private string displayName;

    private string initials;

    private string avatarUrl;

    private string inputText = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public event Action<MessageDataTransferObject> MessageSent;

    public event Action<int, int, bool, bool> BookingRequestUpdate;

    public event Action<int, int> CashAgreementAccept;

    public int CurrentUserId { get; private set; }

    public int ConversationId { get; private set; }

    public ObservableCollection<MessageViewModel> Messages { get; } = new();

    public string DisplayName
    {
        get => displayName;
        set
        {
            displayName = value;
            OnPropertyChanged();
        }
    }

    public string Initials
    {
        get => initials;
        set
        {
            initials = value;
            OnPropertyChanged();
        }
    }

    public string AvatarUrl
    {
        get => avatarUrl;
        set
        {
            avatarUrl = value;
            OnPropertyChanged(nameof(AvatarUrl));
        }
    }

    public string InputText
    {
        get => inputText;
        set
        {
            inputText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSend));
        }
    }

    public ChatViewModel(int currentUser)
    {
        CurrentUserId = currentUser;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void LoadConversation(ConversationPreviewModel conversation, List<MessageDTO> messages, int theirUnreadCount)
    {
        ConversationId = conversation.ConversationId;
        DisplayName = conversation.DisplayName;
        Initials = conversation.Initials;
        AvatarUrl = conversation.AvatarUrl;

        List<MessageDTO> orderedMessages = messages
            .OrderBy(messageItem => messageItem.SentAt)
            .ThenBy(messageItem => messageItem.Id)
            .ToList();

        Messages.Clear();
        for (int messageIndex = 0; messageIndex < orderedMessages.Count; messageIndex++)
        {
            var currentMessage = orderedMessages[messageIndex];
            var newMessageViewModel = new MessageViewModel(currentMessage, CurrentUserId);
            if (messageIndex < orderedMessages.Count - theirUnreadCount)
            {
                newMessageViewModel.IsRead = true;
            }

            Messages.Add(newMessageViewModel);
        }
    }

    public void HandleIncomingMessage(MessageDTO message)
    {
        if (message.ConversationId != ConversationId)
        {
            return;
        }

        bool messageExists = Messages.Any(messageItem => messageItem.Id == message.Id);

        if (messageExists)
        {
            return;
        }

        Messages.Add(new MessageViewModel(message, CurrentUserId));
    }

    public bool CanSend => !string.IsNullOrWhiteSpace(InputText);

    public void SendMessage()
    {
        if (!CanSend)
        {
            return;
        }

        int unassignedIdentifier = -1;

        var messageDataTransferObject = new MessageDTO(
            unassignedIdentifier,
            ConversationId,
            CurrentUserId,
            unassignedIdentifier,
            DateTime.Now,
            InputText,
            MessageType.MessageText,
            null,
            false,
            false,
            false,
            false,
            unassignedIdentifier,
            unassignedIdentifier);

        InputText = string.Empty;
        MessageSent.Invoke(messageDataTransferObject);
    }

    public void ResolveBookingRequest(int messageId, bool accepted)
    {
        var targetMessage = Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        BookingRequestUpdate?.Invoke(messageId, targetMessage.ConversationId, accepted, !accepted);
    }

    public void UpdateCashAgreement(int messageId)
    {
        var targetMessage = Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        CashAgreementAccept?.Invoke(messageId, targetMessage.ConversationId);
    }

    public void ProceedToPayment(int messageId)
    {
        var targetMessage = Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
    }

    public void SendImage(string fileName)
    {
        int unassignedIdentifier = -1;

        var messageDataTransferObject = new MessageDTO(
            unassignedIdentifier,
            ConversationId,
            CurrentUserId,
            unassignedIdentifier,
            DateTime.Now,
            string.Empty,
            MessageType.MessageImage,
            fileName,
            false,
            false,
            false,
            false,
            unassignedIdentifier,
            unassignedIdentifier);

        var newViewModel = new MessageViewModel(messageDataTransferObject, CurrentUserId);
        InputText = string.Empty;
        MessageSent.Invoke(messageDataTransferObject);
    }

    public void RaiseBookingRequestUpdate(int messageId, int conversationId, bool accepted, bool resolved)
    {
        BookingRequestUpdate?.Invoke(messageId, conversationId, accepted, resolved);
    }

    public void RaiseCashAgreementAccept(int messageId, int conversationId)
    {
        CashAgreementAccept?.Invoke(messageId, conversationId);
    }

    public void RaiseMessageSent(MessageDTO message)
    {
        MessageSent?.Invoke(message);
    }
}
