// <copyright file="ChatPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Navigation;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Helpers;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is true ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    public sealed class BoolToBubbleColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush SentBrush = new(new Color { A = 255, R = 0, G = 102, B = 204 });
        private static readonly SolidColorBrush ReceivedBrush = new(new Color { A = 255, R = 60, G = 60, B = 60 });

        public object Convert(object value, Type targetType, object parameter, string language)
            => value is true ? SentBrush : ReceivedBrush;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    internal sealed class ConversationListItem
    {
        public ConversationDTO Conversation { get; init; } = null!;

        public string OtherUserName { get; init; } = string.Empty;

        public string LastMessagePreview { get; init; } = string.Empty;
    }

    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool show = value is true;
            if (parameter is string p && p == "Inverse")
            {
                show = !show;
            }

            return show ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    internal sealed class MessageDisplayItem
    {
        public string SenderLabel { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;

        public string SentAtDisplay { get; init; } = string.Empty;

        public bool IsCurrentUser { get; init; }

        // Rental request fields
        public bool IsRentalRequest { get; init; }

        public int MessageId { get; init; }

        public int RequestId { get; init; }

        public int RentalId { get; init; }

        public bool IsResolved { get; init; }

        public bool IsAccepted { get; init; }

        // Derived visibility helpers
        public bool ShowOwnerActions => IsRentalRequest && !IsResolved && !IsAccepted && !IsCurrentUser;

        public bool ShowRenterCancel => IsRentalRequest && !IsResolved && !IsAccepted && IsCurrentUser;

        public bool ShowAwaitingBadge => IsRentalRequest && !IsResolved && !IsAccepted && IsCurrentUser;

        public bool ShowPendingPaymentBadge => IsRentalRequest && IsAccepted && !IsResolved && !IsCurrentUser;

        public bool ShowProceedToPayment => IsRentalRequest && IsAccepted && !IsResolved && IsCurrentUser;

        public bool ShowCompletedBadge => IsRentalRequest && IsResolved && IsAccepted;

        public bool ShowDeclinedBadge => IsRentalRequest && IsResolved && !IsAccepted;
    }

    public sealed partial class ChatPage : Page
    {
        private readonly IConversationService conversationService;
        private readonly IRequestService requestService;
        private readonly IRentalPaymentService rentalPaymentService;
        private readonly ISessionContext sessionContext;
        private ConversationDTO? selectedConversation;

        public ChatPage()
        {
            this.InitializeComponent();
            this.conversationService = App.Services.GetRequiredService<IConversationService>();
            this.requestService = App.Services.GetRequiredService<IRequestService>();
            this.rentalPaymentService = App.Services.GetRequiredService<IRentalPaymentService>();
            this.sessionContext = App.Services.GetRequiredService<ISessionContext>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            await this.LoadConversationsAsync();
        }

        private async Task LoadConversationsAsync()
        {
            if (!this.sessionContext.IsLoggedIn)
            {
                this.StatusText.Text = "Please sign in to use chat.";
                return;
            }

            var result = await this.conversationService.GetConversationsForUserAsync(this.sessionContext.AccountId);
            if (!result.Success || result.Data == null)
            {
                this.StatusText.Text = result.Error ?? "Could not load conversations.";
                return;
            }

            this.StatusText.Text = result.Data.Count == 0 ? "No conversations yet." : string.Empty;

            int currentPamUserId = this.sessionContext.PamUserId ?? 0;
            var items = result.Data.Select(conv =>
            {
                int otherId = conv.ParticipantUserIds.FirstOrDefault(id => id != currentPamUserId);
                conv.ParticipantDisplayNames.TryGetValue(otherId, out string? otherName);
                var lastMsg = conv.MessageList.OrderByDescending(m => m.SentAt).FirstOrDefault();
                return new ConversationListItem
                {
                    Conversation = conv,
                    OtherUserName = otherName ?? (otherId > 0 ? $"User {otherId}" : "Unknown"),
                    LastMessagePreview = lastMsg?.GetChatMessagePreview() ?? "No messages yet",
                };
            }).ToList();

            this.ConversationsList.ItemsSource = items;
        }

        private async void ConversationsList_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            var item = this.ConversationsList.SelectedItem as ConversationListItem;
            this.selectedConversation = item?.Conversation;
            await this.BindMessagesAsync(this.selectedConversation);
        }

        private List<MessageDisplayItem> BuildMessageItems(ConversationDTO? conversation)
        {
            if (conversation == null)
            {
                return new List<MessageDisplayItem>();
            }

            int currentPamUserId = this.sessionContext.PamUserId ?? 0;
            return conversation.MessageList
                .OrderBy(m => m.SentAt)
                .Select(m =>
                {
                    bool isMe = m.SenderId == currentPamUserId;
                    conversation.ParticipantDisplayNames.TryGetValue(m.SenderId, out string? name);
                    bool isRentalRequest = m.Type == MessageType.MessageRentalRequest;
                    return new MessageDisplayItem
                    {
                        SenderLabel = isMe ? "You" : (name ?? $"User {m.SenderId}"),
                        Content = m.Content,
                        SentAtDisplay = m.SentAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                        IsCurrentUser = isMe,
                        IsRentalRequest = isRentalRequest,
                        MessageId = m.Id,
                        RequestId = isRentalRequest
                            ? RentalRequestMessageHelper.ResolveRequestId(m.RequestId, m.Content)
                            : -1,
                        RentalId = RentalRequestMessageHelper.ResolveRentalId(m.RentalId, m.Content),
                        IsResolved = m.IsResolved,
                        IsAccepted = m.IsAccepted,
                    };
                })
                .ToList();
        }

        private async void AcceptRequest_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button btn && btn.Tag is MessageDisplayItem item && item.RequestId > 0)
            {
                var result = await this.requestService.OfferGameAsync(item.RequestId, new RequestActionDTO { AccountId = this.sessionContext.AccountId });
                await this.RefreshSelectedConversationAsync(result.Success ? null : (result.Error ?? "Could not accept the request."));
            }
        }

        private async void ProceedToPayment_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is not Button { Tag: MessageDisplayItem item } || this.selectedConversation is null)
            {
                return;
            }

            int rentalId = item.RentalId;
            if (rentalId <= 0)
            {
                this.StatusText.Text = "Rental details are not ready yet. Refresh the conversation after the owner accepts.";
                await this.RefreshSelectedConversationAsync(null);
                return;
            }

            var checkoutResult = await this.rentalPaymentService.GetCheckoutSummaryAsync(rentalId, this.sessionContext.AccountId);
            if (!checkoutResult.Success || checkoutResult.Data is null)
            {
                this.StatusText.Text = checkoutResult.Error ?? "Could not load checkout details.";
                return;
            }

            var paymentWindow = new Window();
            var frame = new Frame();
            paymentWindow.Content = frame;
            paymentWindow.Title = "Complete rental payment";
            paymentWindow.Activate();

            frame.Navigate(typeof(DeliveryView), new DeliveryNavigationArgs
            {
                Checkout = checkoutResult.Data,
                ChatRequestId = item.RequestId,
                MessageId = item.MessageId,
                HostWindow = paymentWindow,
            });

            paymentWindow.Closed += async (_, _) => await this.RefreshSelectedConversationAsync(null);
        }

        private async void DeclineRequest_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button btn && btn.Tag is MessageDisplayItem item && item.RequestId > 0)
            {
                var result = await this.requestService.DenyRequestAsync(item.RequestId, new RequestActionDTO { AccountId = this.sessionContext.AccountId });
                await this.RefreshSelectedConversationAsync(result.Success ? null : (result.Error ?? "Could not decline the request."));
            }
        }

        private async void CancelRequest_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button btn && btn.Tag is MessageDisplayItem item && item.RequestId > 0)
            {
                var result = await this.requestService.CancelRequestAsync(item.RequestId, new RequestActionDTO { AccountId = this.sessionContext.AccountId });
                await this.RefreshSelectedConversationAsync(result.Success ? null : (result.Error ?? "Could not cancel the request."));
            }
        }

        private async Task RefreshSelectedConversationAsync(string? errorMessage)
        {
            if (errorMessage != null)
            {
                this.StatusText.Text = errorMessage;
                return;
            }

            if (this.selectedConversation != null)
            {
                var refreshed = await this.conversationService.GetConversationByIdAsync(this.selectedConversation.Id);
                if (refreshed.Success && refreshed.Data != null)
                {
                    this.selectedConversation = refreshed.Data;
                    await this.BindMessagesAsync(refreshed.Data);
                }
            }
        }

        private async Task BindMessagesAsync(ConversationDTO? conversation)
        {
            this.MessagesList.ItemsSource = this.BuildMessageItems(conversation);
            await this.ScrollMessagesToBottomAsync();
        }

        private async Task ScrollMessagesToBottomAsync()
        {
            await Task.Delay(50);
            if (this.MessagesList.Items.Count > 0)
            {
                var last = this.MessagesList.Items[this.MessagesList.Items.Count - 1];
                this.MessagesList.ScrollIntoView(last);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (this.selectedConversation == null || string.IsNullOrWhiteSpace(this.MessageTextBox.Text))
            {
                return;
            }

            int senderPamUserId = this.sessionContext.PamUserId ?? 0;
            if (senderPamUserId == 0)
            {
                this.StatusText.Text = "Chat requires a linked legacy user id for this account.";
                return;
            }

            int receiverPamUserId = this.selectedConversation.ParticipantUserIds.FirstOrDefault(id => id != senderPamUserId);
            if (receiverPamUserId == 0)
            {
                this.StatusText.Text = "Could not find the other participant.";
                return;
            }

            var message = new MessageDataTransferObject(
                0,
                this.selectedConversation.Id,
                senderPamUserId,
                receiverPamUserId,
                DateTime.UtcNow,
                this.MessageTextBox.Text.Trim(),
                MessageType.MessageText,
                string.Empty,
                false,
                false,
                false,
                false,
                -1,
                -1);

            var sendResult = await this.conversationService.SendMessageAsync(message);
            if (!sendResult.Success)
            {
                this.StatusText.Text = sendResult.Error ?? "Message could not be sent.";
                return;
            }

            this.MessageTextBox.Text = string.Empty;
            var refreshed = await this.conversationService.GetConversationByIdAsync(this.selectedConversation.Id);
            if (refreshed.Success && refreshed.Data != null)
            {
                this.selectedConversation = refreshed.Data;
                await this.BindMessagesAsync(refreshed.Data);
            }
        }
    }
}
