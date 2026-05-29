using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class ChatPage : Page
    {
        private readonly IConversationService conversationService;
        private readonly ISessionContext sessionContext;
        private ConversationDTO? selectedConversation;

        public ChatPage()
        {
            this.InitializeComponent();
            this.conversationService = App.Services.GetRequiredService<IConversationService>();
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
            this.ConversationsList.ItemsSource = result.Data;
        }

        private void ConversationsList_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            this.selectedConversation = this.ConversationsList.SelectedItem as ConversationDTO;
            this.MessagesList.ItemsSource = this.selectedConversation?.MessageList.OrderBy(message => message.SentAt).ToList();
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
                this.MessagesList.ItemsSource = refreshed.Data.MessageList.OrderBy(item => item.SentAt).ToList();
            }
        }
    }
}
