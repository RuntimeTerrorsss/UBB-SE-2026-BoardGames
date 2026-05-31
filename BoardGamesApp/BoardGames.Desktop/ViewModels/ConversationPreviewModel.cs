// <copyright file="ConversationPreviewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BoardGames.Shared.DTO;

namespace BoardGames.Desktop.ViewModels;

public class ConversationPreviewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int ConversationId { get; init; }

    public string DisplayName { get; init; }

    public string Initials { get; init; }

    public string AvatarUrl { get; init; }

    private string lastMessageText;

    public string LastMessageText
    {
        get => lastMessageText;
        set
        {
            lastMessageText = value;
            OnPropertyChanged();
        }
    }

    private DateTime timestamp;

    public DateTime Timestamp
    {
        get => timestamp;
        set
        {
            timestamp = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimestampString));
        }
    }

    private int unreadCount;

    public int UnreadCount
    {
        get => unreadCount;
        set
        {
            unreadCount = value;
            OnPropertyChanged();
        }
    }

    public string TimestampString => timestamp.ToString("HH:mm");

    public ConversationPreviewModel(
        int conversationId,
        string displayName,
        string initials,
        string lastMessageTextInput,
        DateTime timestampInput,
        int unreadCountInput,
        string avatarUrl)
    {
        ConversationId = conversationId;
        DisplayName = displayName;
        Initials = initials;
        AvatarUrl = avatarUrl;
        lastMessageText = lastMessageTextInput;
        timestamp = timestampInput;
        unreadCount = unreadCountInput;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
