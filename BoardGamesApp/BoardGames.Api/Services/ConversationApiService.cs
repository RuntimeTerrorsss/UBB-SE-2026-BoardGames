using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class ConversationApiService : IConversationApiService
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IAccountRepository accountRepository;

        public ConversationApiService(IConversationRepository conversationRepository, IAccountRepository accountRepository)
        {
            this.conversationRepository = conversationRepository;
            this.accountRepository = accountRepository;
        }

        public async Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId)
        {
            int pamUserId = await GetPamUserIdAsync(accountId);
            var conversations = await conversationRepository.GetConversationsForUser(pamUserId);
            return conversations.Select(MapConversationToDTO).ToList();
        }

        public async Task<ConversationDTO?> GetConversationById(int conversationId)
        {
            try
            {
                var conversation = await conversationRepository.GetConversationById(conversationId);
                return MapConversationToDTO(conversation);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public async Task<MessageDataTransferObject> SendMessage(MessageDataTransferObject dto)
        {
            var entity = MapDtoToEntity(dto);
            entity.MessageId = 0;
            var persisted = await conversationRepository.HandleNewMessage(entity);
            return MapEntityToDto(persisted);
        }

        public async Task<MessageDataTransferObject?> UpdateMessage(MessageDataTransferObject dto)
        {
            var entity = MapDtoToEntity(dto);
            var updated = await conversationRepository.HandleMessageUpdate(entity);
            return updated is null ? null : MapEntityToDto(updated);
        }

        public async Task HandleReadReceipt(ReadReceiptDTO dto)
        {
            await conversationRepository.HandleReadReceipt(dto);
        }

        public async Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB)
        {
            int pamUserIdA = await GetPamUserIdAsync(accountIdA);
            int pamUserIdB = await GetPamUserIdAsync(accountIdB);
            return await conversationRepository.FindOrCreateConversationBetweenUsers(pamUserIdA, pamUserIdB);
        }

        public async Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end)
        {
            int renterPamId = await GetPamUserIdAsync(renterAccountId);
            int ownerPamId = await GetPamUserIdAsync(ownerAccountId);
            int conversationId = await conversationRepository.FindOrCreateConversationBetweenUsers(renterPamId, ownerPamId);

            string content = $"[req:{requestId}] Rental request for {gameName} from {start:d} to {end:d}.";
            var message = new RentalRequestMessage
            {
                ConversationId = conversationId,
                MessageSenderId = renterPamId,
                MessageReceiverId = ownerPamId,
                RentalRequestId = 0,
                RequestContent = content,
                MessageContentAsString = content,
                MessageSentTime = DateTime.UtcNow,
                IsRequestResolved = false,
                IsRequestAccepted = false,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            await conversationRepository.HandleNewMessage(message);
        }

        public async Task FinalizeRentalRequestMessage(int requestId, bool accepted)
        {
            var message = await conversationRepository.FindRentalRequestMessageByRequestId(requestId);
            if (message is null)
            {
                return;
            }

            message.IsRequestResolved = true;
            message.IsRequestAccepted = accepted;
            await conversationRepository.HandleMessageUpdate(message);
        }

        public async Task<MessageDataTransferObject?> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var created = await conversationRepository.CreateCashAgreementMessage(parentMessageId, paymentId);
            return created is null ? null : MapEntityToDto(created);
        }

        private async Task<int> GetPamUserIdAsync(Guid accountId)
        {
            var user = await accountRepository.GetByIdAsync(accountId);
            return user?.PamUserId ?? throw new KeyNotFoundException($"User with account id {accountId} not found.");
        }

        private static ConversationDTO MapConversationToDTO(Conversation conversation)
        {
            var messages = conversation.Messages?.Select(MapEntityToDto).ToList() ?? new List<MessageDataTransferObject>();
            var participantUserIds = conversation.Participants?.Select(p => p.UserId).ToList() ?? new List<int>();
            var lastRead = conversation.Participants?
                .Where(p => p.LastMessageReadTime.HasValue)
                .ToDictionary(p => p.UserId, p => p.LastMessageReadTime!.Value)
                ?? new Dictionary<int, DateTime>();
            return new ConversationDTO(
                conversation.ConversationId,
                participantUserIds,
                messages,
                lastRead);
        }

        private static MessageDataTransferObject MapEntityToDto(Message message)
        {
            const int defaultMissingIdentifier = -1;

            MessageType messageType = message switch
            {
                TextMessage => MessageType.MessageText,
                ImageMessage => MessageType.MessageImage,
                RentalRequestMessage => MessageType.MessageRentalRequest,
                CashAgreementMessage => MessageType.MessageCashAgreement,
                SystemMessage => MessageType.MessageSystem,
                _ => throw new ArgumentOutOfRangeException(nameof(message), message.GetType().Name, "Unknown message subtype."),
            };

            string content = message switch
            {
                TextMessage textMessage => textMessage.TextMessageContent ?? textMessage.MessageContentAsString ?? string.Empty,
                RentalRequestMessage rentalMsg => rentalMsg.RequestContent ?? rentalMsg.MessageContentAsString ?? string.Empty,
                SystemMessage systemMsg => systemMsg.MessageContent ?? systemMsg.MessageContentAsString ?? string.Empty,
                _ => message.MessageContentAsString ?? string.Empty,
            };

            return new MessageDataTransferObject(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: content,
                Type: messageType,
                ImageUrl: message is ImageMessage img ? img.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rrm ? rrm.IsRequestResolved
                          : message is CashAgreementMessage cam ? cam.IsCashAgreementResolved
                          : false,
                IsAccepted: message is RentalRequestMessage ram ? ram.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage camb ? camb.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cams ? cams.IsCashAgreementAcceptedBySeller : false,
                RequestId: message is RentalRequestMessage rrm2 ? rrm2.RentalRequestId : defaultMissingIdentifier,
                PaymentId: message is CashAgreementMessage cam2 ? cam2.CashPaymentId : defaultMissingIdentifier);
        }

        private static Message MapDtoToEntity(MessageDataTransferObject dto)
        {
            return dto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    TextMessageContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    MessageImageUrl = dto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    RentalRequestId = dto.RequestId,
                    IsRequestResolved = dto.IsResolved,
                    IsRequestAccepted = dto.IsAccepted,
                    RequestContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    CashPaymentId = dto.PaymentId,
                    IsCashAgreementResolved = dto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = dto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = dto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    MessageContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(dto.Type), dto.Type, "Unsupported message type."),
            };
        }
    }
}
