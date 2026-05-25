// <copyright file="ChatViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

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
        get => this.displayName;
        set
        {
            this.displayName = value;
            this.OnPropertyChanged();
        }
    }

    public string Initials
    {
        get => this.initials;
        set
        {
            this.initials = value;
            this.OnPropertyChanged();
        }
    }

    public string AvatarUrl
    {
        get => this.avatarUrl;
        set
        {
            this.avatarUrl = value;
            this.OnPropertyChanged(nameof(this.AvatarUrl));
        }
    }

    public string InputText
    {
        get => this.inputText;
        set
        {
            this.inputText = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.CanSend));
        }
    }

    public ChatViewModel(int currentUser)
    {
        this.CurrentUserId = currentUser;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void LoadConversation(ConversationPreviewModel conversation, List<MessageDataTransferObject> messages, int theirUnreadCount)
    {
        this.ConversationId = conversation.ConversationId;
        this.DisplayName = conversation.DisplayName;
        this.Initials = conversation.Initials;
        this.AvatarUrl = conversation.AvatarUrl;

        List<MessageDataTransferObject> orderedMessages = messages
            .OrderBy(messageItem => messageItem.SentAt)
            .ThenBy(messageItem => messageItem.Id)
            .ToList();

        this.Messages.Clear();
        for (int messageIndex = 0; messageIndex < orderedMessages.Count; messageIndex++)
        {
            var currentMessage = orderedMessages[messageIndex];
            var newMessageViewModel = new MessageViewModel(currentMessage, this.CurrentUserId);
            if (messageIndex < orderedMessages.Count - theirUnreadCount)
            {
                newMessageViewModel.IsRead = true;
            }

            this.Messages.Add(newMessageViewModel);
        }
    }

    public void HandleIncomingMessage(MessageDataTransferObject message)
    {
        if (message.ConversationId != this.ConversationId)
        {
            return;
        }

        bool messageExists = this.Messages.Any(messageItem => messageItem.Id == message.Id);

        if (messageExists)
        {
            return;
        }

        this.Messages.Add(new MessageViewModel(message, this.CurrentUserId));
    }

    public bool CanSend => !string.IsNullOrWhiteSpace(this.InputText);

    public void SendMessage()
    {
        if (!this.CanSend)
        {
            return;
        }

        int unassignedIdentifier = -1;

        var messageDataTransferObject = new MessageDataTransferObject(
            unassignedIdentifier,
            this.ConversationId,
            this.CurrentUserId,
            unassignedIdentifier,
            DateTime.Now,
            this.InputText,
            MessageType.MessageText,
            null,
            false,
            false,
            false,
            false,
            unassignedIdentifier,
            unassignedIdentifier);

        this.InputText = string.Empty;
        this.MessageSent.Invoke(messageDataTransferObject);
    }

    public void ResolveBookingRequest(int messageId, bool accepted)
    {
        var targetMessage = this.Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        this.BookingRequestUpdate?.Invoke(messageId, targetMessage.ConversationId, accepted, !accepted);
    }

    public void UpdateCashAgreement(int messageId)
    {
        var targetMessage = this.Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        this.CashAgreementAccept?.Invoke(messageId, targetMessage.ConversationId);
    }

    public void ProceedToPayment(int messageId)
    {
        var targetMessage = this.Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
    }

    public void SendImage(string fileName)
    {
        int unassignedIdentifier = -1;

        var messageDataTransferObject = new MessageDataTransferObject(
            unassignedIdentifier,
            this.ConversationId,
            this.CurrentUserId,
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

        var newViewModel = new MessageViewModel(messageDataTransferObject, this.CurrentUserId);
        this.InputText = string.Empty;
        this.MessageSent.Invoke(messageDataTransferObject);
    }

    public void RaiseBookingRequestUpdate(int messageId, int conversationId, bool accepted, bool resolved)
    {
        this.BookingRequestUpdate?.Invoke(messageId, conversationId, accepted, resolved);
    }

    public void RaiseCashAgreementAccept(int messageId, int conversationId)
    {
        this.CashAgreementAccept?.Invoke(messageId, conversationId);
    }

    public void RaiseMessageSent(MessageDataTransferObject message)
    {
        this.MessageSent?.Invoke(message);
    }
}
