// <copyright file="MessageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace BookingBoardGames.Src.ViewModels;

public class MessageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public int Id { get; init; }

    public int ConversationId { get; init; }

    public int SenderId { get; init; }

    public MessageType Type { get; init; }

    public string Content { get; init; }

    public bool IsMine { get; init; }

    public DateTime SentAt { get; init; }

    public string ImageUrl { get; init; }

    public int RequestId { get; init; }

    public string TimestampString => this.SentAt.ToString("HH:mm");

    private bool isResolved;

    public bool IsResolved
    {
        get => this.isResolved;
        set
        {
            this.isResolved = value;
            this.OnPropertyChanged();
        }
    }

    public bool IsAccepted { get; set; }

    private int[] acceptedBy = Array.Empty<int>();

    public int[] AcceptedBy
    {
        get => this.acceptedBy;
        set
        {
            this.acceptedBy = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.BothAccepted));
        }
    }

    private readonly int requiredAcceptanceCount = 2;

    public bool BothAccepted => this.acceptedBy?.Length == this.requiredAcceptanceCount;

    private bool isRead;

    public bool IsRead
    {
        get => this.isRead;
        set
        {
            this.isRead = value;
            this.OnPropertyChanged();
        }
    }

    public MessageViewModel(MessageDataTransferObject message, int currentUserId)
    {
        int systemUserIdentifier = 0;

        this.Id = message.Id;
        this.ConversationId = message.ConversationId;
        this.SenderId = message.SenderId;
        this.Type = message.Type;
        this.Content = message.Content;
        this.IsMine = message.SenderId == currentUserId;
        this.SentAt = message.SentAt;
        this.ImageUrl = message.ImageUrl;
        this.RequestId = message.RequestId;
        this.IsAccepted = message.IsAccepted;
        this.isResolved = message.IsResolved;
        this.acceptedBy = new int[] { message.IsAcceptedByBuyer ? message.ReceiverId : systemUserIdentifier, message.IsAcceptedBySeller ? message.SenderId : systemUserIdentifier };
        this.isRead = false;
    }

    public HorizontalAlignment IsMineToAlignment(bool isMine)
        => isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public Brush IsMineToBackground(bool isMine)
        => isMine
            ? (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"]
            : (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];

    public Brush IsMineToForeground(bool isMine)
        => isMine
            ? new SolidColorBrush(Microsoft.UI.Colors.White)
            : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];

    public CornerRadius IsMineToCornerRadius(bool isMine)
    {
        double curvedCornerRadius = 12;
        double flatCornerRadius = 2;
        return isMine ? new CornerRadius(curvedCornerRadius, curvedCornerRadius, flatCornerRadius, curvedCornerRadius) : new CornerRadius(curvedCornerRadius, curvedCornerRadius, curvedCornerRadius, flatCornerRadius);
    }

    public Thickness IsMineToTheirsOnlyBorderThickness(bool isMine)
    {
        double noBorderThickness = 0;
        double defaultBorderThickness = 1;
        return isMine ? new Thickness(noBorderThickness) : new Thickness(defaultBorderThickness);
    }
}
