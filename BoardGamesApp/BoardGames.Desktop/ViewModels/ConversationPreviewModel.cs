// <copyright file="ConversationPreviewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BookingBoardGames.Sharing.DTO;

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
        get => this.lastMessageText;
        set
        {
            this.lastMessageText = value;
            this.OnPropertyChanged();
        }
    }

    private DateTime timestamp;

    public DateTime Timestamp
    {
        get => this.timestamp;
        set
        {
            this.timestamp = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.TimestampString));
        }
    }

    private int unreadCount;

    public int UnreadCount
    {
        get => this.unreadCount;
        set
        {
            this.unreadCount = value;
            this.OnPropertyChanged();
        }
    }

    public string TimestampString => this.timestamp.ToString("HH:mm");

    public ConversationPreviewModel(
        int conversationId,
        string displayName,
        string initials,
        string lastMessageTextInput,
        DateTime timestampInput,
        int unreadCountInput,
        string avatarUrl)
    {
        this.ConversationId = conversationId;
        this.DisplayName = displayName;
        this.Initials = initials;
        this.AvatarUrl = avatarUrl;
        this.lastMessageText = lastMessageTextInput;
        this.timestamp = timestampInput;
        this.unreadCount = unreadCountInput;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
