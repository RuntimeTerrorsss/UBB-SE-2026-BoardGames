// <copyright file="MessageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.ViewModels;

public class MessageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public int Id { get; init; }

    public int ConversationId { get; init; }

    public int SenderId { get; init; }

    public MessageType Type { get; init; }

    public string Content { get; init; }

    public bool IsMine { get; init; }

    public DateTime SentAt { get; init; }

    public string ImageUrl { get; init; }

    public int RequestId { get; init; }

    public string TimestampString => SentAt.ToString("HH:mm");

    private bool isResolved;

    public bool IsResolved
    {
        get => isResolved;
        set
        {
            isResolved = value;
            OnPropertyChanged();
        }
    }

    public bool IsAccepted { get; set; }

    private int[] acceptedBy = Array.Empty<int>();

    public int[] AcceptedBy
    {
        get => acceptedBy;
        set
        {
            acceptedBy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BothAccepted));
        }
    }

    private readonly int requiredAcceptanceCount = 2;

    public bool BothAccepted => acceptedBy?.Length == requiredAcceptanceCount;

    private bool isRead;

    public bool IsRead
    {
        get => isRead;
        set
        {
            isRead = value;
            OnPropertyChanged();
        }
    }

    public MessageViewModel(MessageDataTransferObject message, int currentUserId)
    {
        int systemUserIdentifier = 0;

        Id = message.Id;
        ConversationId = message.ConversationId;
        SenderId = message.SenderId;
        Type = message.Type;
        Content = message.Content;
        IsMine = message.SenderId == currentUserId;
        SentAt = message.SentAt;
        ImageUrl = message.ImageUrl;
        RequestId = message.RequestId;
        IsAccepted = message.IsAccepted;
        isResolved = message.IsResolved;
        acceptedBy = new int[] { message.IsAcceptedByBuyer ? message.ReceiverId : systemUserIdentifier, message.IsAcceptedBySeller ? message.SenderId : systemUserIdentifier };
        isRead = false;
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
