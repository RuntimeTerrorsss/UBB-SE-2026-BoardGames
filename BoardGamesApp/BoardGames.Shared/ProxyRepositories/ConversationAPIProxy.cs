// <copyright file="ConversationAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using System.Text.Json;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;

namespace BoardGames.Shared.ProxyRepositories
{
    public class ConversationAPIProxy : IConversationRepository
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            //Converters = { new MessageJsonConverter() }
        };

        public ConversationAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<Conversation>> GetConversationsForUser(int userId)
        {
            return await httpClient.GetFromJsonAsync<List<Conversation>>(
                       $"conversation/user/{userId}", JsonOptions)
                   ?? new List<Conversation>();
        }

        public async Task<Conversation> GetConversationById(int conversationId)
        {
            var response = await httpClient.GetAsync($"conversation/{conversationId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Conversation>(JsonOptions)
                   ?? throw new InvalidOperationException($"Conversation {conversationId} was not found.");
        }

        public async Task<IReadOnlyList<int>> GetParticipantUserIds(int conversationId)
        {
            return await httpClient.GetFromJsonAsync<List<int>>(
                       $"conversation/{conversationId}/participants", JsonOptions)
                   ?? new List<int>();
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            var response = await httpClient.PostAsJsonAsync(
                "conversation",
                new { SenderId = senderId, ReceiverId = receiverId },
                JsonOptions);
            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadAsStringAsync();
            var createdConversation = JsonSerializer.Deserialize<Conversation>(raw, JsonOptions);
            if (createdConversation is not null && createdConversation.ConversationId > 0)
            {
                return createdConversation.ConversationId;
            }

            return JsonSerializer.Deserialize<int>(raw, JsonOptions);
        }

        public async Task<int> FindOrCreateConversationBetweenUsers(int userIdA, int userIdB)
        {
            if (userIdA <= 0 || userIdB <= 0 || userIdA == userIdB)
            {
                throw new ArgumentException("Participants must be two distinct valid user ids.");
            }

            var conversations = await GetConversationsForUser(userIdA);
            foreach (var conversation in conversations)
            {
                var participants = conversation.Participants;
                if (participants is null || participants.Count != 2)
                {
                    continue;
                }

                var participantIds = new HashSet<int>(participants.Select(participantItem => participantItem.UserId));

                if (participantIds.Contains(userIdA) && participantIds.Contains(userIdB))
                {
                    return conversation.ConversationId;
                }
            }

            return await CreateConversation(userIdA, userIdB);
        }

        public async Task<Message> HandleNewMessage(Message message)
        {
            var messageDto = MessageToMessageDto(message);
            var response = await httpClient.PostAsJsonAsync("conversation/messages", messageDto, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var participantIds = await this.GetParticipantUserIds(message.ConversationId);
                var fallbackReceiverId = participantIds
                    .FirstOrDefault(participantId => participantId != message.MessageSenderId);

                if (fallbackReceiverId > 0 && fallbackReceiverId != message.MessageReceiverId)
                {
                    messageDto = messageDto with { ReceiverId = fallbackReceiverId };
                    response = await httpClient.PostAsJsonAsync("conversation/messages", messageDto, JsonOptions);
                }
            }

            response.EnsureSuccessStatusCode();

            var persistedDto = await response.Content.ReadFromJsonAsync<MessageDto>(JsonOptions)
                               ?? throw new InvalidOperationException("Failed to create message.");
            return MessageDtoToMessage(persistedDto);
        }

        public async Task<Message?> HandleMessageUpdate(Message message)
        {
            var messageDto = MessageToMessageDto(message);
            var response = await httpClient.PutAsJsonAsync(
                "conversation/messages", messageDto, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var persistedDto = await response.Content.ReadFromJsonAsync<MessageDto>(JsonOptions);
            return persistedDto is null ? null : MessageDtoToMessage(persistedDto);
        }

        public async Task HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var response = await httpClient.PostAsJsonAsync(
                "conversation/readreceipt", readReceipt, JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Message?> HandleRentalRequestFinalization(int messageId)
        {
            var response = await httpClient.PostAsync(
                $"conversation/rental/finalize/{messageId}", null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var updatedDto = await response.Content.ReadFromJsonAsync<MessageDto>(JsonOptions);
            return updatedDto is null ? null : MessageDtoToMessage(updatedDto);
        }

        public async Task<Message?> CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var response = await httpClient.PostAsync(
                $"conversation/cash/{messageIdOfParentRentalRequestMessage}/{paymentId}", null);
            if (!response.IsSuccessStatusCode) return null;
            var resultDto = await response.Content.ReadFromJsonAsync<MessageDto>(JsonOptions);
            return resultDto is null ? null : MessageDtoToMessage(resultDto);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private MessageDto MessageToMessageDto(Message message)
        {
            int defaultMissingIdentifier = -1;

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
                RentalRequestMessage rentalForContent => rentalForContent.RequestContent ?? rentalForContent.MessageContentAsString ?? string.Empty,
                SystemMessage systemMessage => systemMessage.MessageContent ?? systemMessage.MessageContentAsString ?? string.Empty,
                _ => message.MessageContentAsString ?? string.Empty,
            };

            return new MessageDto(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: content,
                Type: messageType,
                ImageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rentalResolvedMessage ? rentalResolvedMessage.IsRequestResolved
                          : message is CashAgreementMessage cashResolvedMessage ? cashResolvedMessage.IsCashAgreementResolved
                          : false,
                IsAccepted: message is RentalRequestMessage rentalAcceptedMessage ? rentalAcceptedMessage.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cashBuyerMessage ? cashBuyerMessage.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cashSellerMessage ? cashSellerMessage.IsCashAgreementAcceptedBySeller : false,
                RequestId: message is RentalRequestMessage rentalRequestMessage ? rentalRequestMessage.RentalRequestId : defaultMissingIdentifier,
                PaymentId: message is CashAgreementMessage cashPaymentMessage ? cashPaymentMessage.CashPaymentId : defaultMissingIdentifier);
        }

        private Message MessageDtoToMessage(MessageDto messageDto)
        {
            return messageDto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    TextMessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageImageUrl = messageDto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    RentalRequestId = messageDto.RequestId,
                    IsRequestResolved = messageDto.IsResolved,
                    IsRequestAccepted = messageDto.IsAccepted,
                    RequestContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    CashPaymentId = messageDto.PaymentId,
                    IsCashAgreementResolved = messageDto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = messageDto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = messageDto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(messageDto.Type), messageDto.Type, "Unsupported message type."),
            };
        }

        private record MessageDto(
            int Id,
            int ConversationId,
            int SenderId,
            int ReceiverId,
            DateTime SentAt,
            string? Content,
            MessageType Type,
            string? ImageUrl,
            bool IsResolved,
            bool IsAccepted,
            bool IsAcceptedByBuyer,
            bool IsAcceptedBySeller,
            int RequestId,
            int PaymentId);
    }
}
