using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Desktop.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private readonly IConversationService conversationService;
        private readonly ISessionContext sessionContext;
        private ConversationDTO? currentConversation;

        public ObservableCollection<MessageDataTransferObject> Messages { get; } = new();

        public int ConversationId { get; private set; }

        public string DisplayName { get; private set; } = "Chat";

        public ChatViewModel(IConversationService conversationService, ISessionContext sessionContext)
        {
            this.conversationService = conversationService;
            this.sessionContext = sessionContext;
        }

        public async Task LoadConversationAsync(int conversationId)
        {
            IsLoading = true;
            ConversationId = conversationId;
            var result = await conversationService.GetConversationByIdAsync(conversationId);

            if (result.Success && result.Data != null)
            {
                currentConversation = result.Data;
                Messages.Clear();
                foreach (var msg in currentConversation.MessageList.OrderBy(m => m.SentAt))
                {
                    Messages.Add(msg);
                }
            }

            IsLoading = false;
        }

        public async Task SendTextMessageAsync(string text)
        {
            await SendMessageInternal(text, MessageType.MessageText, string.Empty);
        }

        public async Task SendImageAsync(string imageUrl)
        {
            await SendMessageInternal(string.Empty, MessageType.MessageImage, imageUrl);
        }

        private async Task SendMessageInternal(string content, MessageType type, string imageUrl)
        {
            if (currentConversation == null) return;

            int currentUserId = sessionContext.PamUserId ?? 0;
            if (currentUserId == 0)
            {
                ErrorMessage = "Chat requires a linked legacy user id for this account.";
                return;
            }

            var receiverId = currentConversation.ParticipantUserIds.FirstOrDefault(id => id != currentUserId);

            var messageDto = new MessageDataTransferObject(
                0, ConversationId, currentUserId, receiverId, DateTime.UtcNow,
                content, type, imageUrl, false, false, false, false, -1, -1);

            var result = await conversationService.SendMessageAsync(messageDto);
            if (result.Success)
            {
                Messages.Add(result.Data!);
            }
        }

        public async Task UpdateMessageAsync(MessageDataTransferObject updatedMessage)
        {
            var result = await conversationService.UpdateMessageAsync(updatedMessage);
            if (result.Success)
            {
                var index = Messages.ToList().FindIndex(m => m.Id == updatedMessage.Id);
                if (index != -1) Messages[index] = result.Data!;
            }
        }

        public async Task SendReadReceiptAsync()
        {
            if (currentConversation == null) return;

            var dto = new ReadReceiptDTO(ConversationId, sessionContext.PamUserId ?? 0, 0, DateTime.UtcNow);
            await conversationService.SendReadReceiptAsync(dto);
        }
    }
}
