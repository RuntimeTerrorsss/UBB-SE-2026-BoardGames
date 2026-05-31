// <copyright file="ConversationApiService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Helpers;

namespace BoardGames.Api.Services
{
    public class ConversationApiService : IConversationApiService
    {
        private const int NewMessageIdentifier = 0;
        private const int MissingLinkedIdentifier = -1;

        private readonly IConversationRepository conversationRepository;
        private readonly IAccountRepository accountRepository;

        public ConversationApiService(IConversationRepository conversationRepository, IAccountRepository accountRepository)
        {
            this.conversationRepository = conversationRepository;
            this.accountRepository = accountRepository;
        }

        public async Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId)
        {
            int pamUserId = await this.GetPamUserIdAsync(accountId);
            var conversations = await this.conversationRepository.GetConversationsForUser(pamUserId);
            var dtos = new List<ConversationDTO>();
            foreach (var conversation in conversations)
            {
                dtos.Add(await this.MapConversationToDTOAsync(conversation));
            }

            return dtos;
        }

        public async Task<ConversationDTO?> GetConversationById(int conversationId)
        {
            try
            {
                var conversation = await this.conversationRepository.GetConversationById(conversationId);
                return await this.MapConversationToDTOAsync(conversation);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public async Task<MessageDataTransferObject> SendMessage(MessageDataTransferObject dto)
        {
            var entity = MapDtoToEntity(dto);
            entity.MessageId = NewMessageIdentifier;
            var persisted = await conversationRepository.HandleNewMessage(entity);
            return MapEntityToDto(persisted);
        }

        public async Task<MessageDataTransferObject?> UpdateMessage(MessageDataTransferObject dto)
        {
            var entity = MapDtoToEntity(dto);
            var updated = await this.conversationRepository.HandleMessageUpdate(entity);
            return updated is null ? null : MapEntityToDto(updated);
        }

        public async Task HandleReadReceipt(BoardGames.Data.Models.ReadReceiptDTO dto)
        {
            await this.conversationRepository.HandleReadReceipt(dto);
        }

        public async Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB)
        {
            int pamUserIdA = await this.GetPamUserIdAsync(accountIdA);
            int pamUserIdB = await this.GetPamUserIdAsync(accountIdB);
            return await this.conversationRepository.FindOrCreateConversationBetweenUsers(pamUserIdA, pamUserIdB);
        }

        public async Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end)
        {
            int renterPamId = await this.GetPamUserIdAsync(renterAccountId);
            int ownerPamId = await this.GetPamUserIdAsync(ownerAccountId);
            int conversationId = await this.conversationRepository.FindOrCreateConversationBetweenUsers(renterPamId, ownerPamId);

            string content = $"[req:{requestId}] Rental request for {gameName} from {start:d} to {end:d}.";
            var message = new RentalRequestMessage
            {
                ConversationId = conversationId,
                MessageSenderId = renterPamId,
                MessageReceiverId = ownerPamId,
                RentalRequestId = null,
                RequestContent = content,
                MessageContentAsString = content,
                MessageSentTime = DateTime.UtcNow,
                IsRequestResolved = false,
                IsRequestAccepted = false,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            await this.conversationRepository.HandleNewMessage(message);
        }

        public async Task AcceptRentalRequestMessage(int requestId, int rentalId)
        {
            await this.conversationRepository.AcceptRentalRequestByRequestId(requestId, rentalId);
        }

        public async Task FinalizeRentalRequestMessage(int requestId, bool accepted)
        {
            var message = await this.conversationRepository.FindRentalRequestMessageByRequestId(requestId);
            if (message is null)
            {
                return;
            }

            await this.conversationRepository.FinalizeRentalRequestByMessageId(message.MessageId, accepted);
        }

        public async Task<MessageDataTransferObject?> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var created = await this.conversationRepository.CreateCashAgreementMessage(parentMessageId, paymentId);
            return created is null ? null : MapEntityToDto(created);
        }

        private async Task<int> GetPamUserIdAsync(Guid accountId)
        {
            var user = await this.accountRepository.GetByIdAsync(accountId);
            return user?.PamUserId ?? throw new KeyNotFoundException($"User with account id {accountId} not found.");
        }

        private async Task<ConversationDTO> MapConversationToDTOAsync(Conversation conversation)
        {
            var messages = conversation.Messages?.Select(MapEntityToDto).ToList() ?? new List<MessageDataTransferObject>();
            var participantUserIds = conversation.Participants?.Select(participant => participant.UserId).ToList() ?? new List<int>();
            var lastRead = conversation.Participants?
                .Where(participant => participant.LastMessageReadTime.HasValue)
                .ToDictionary(participant => participant.UserId, participant => participant.LastMessageReadTime!.Value)
                ?? new Dictionary<int, DateTime>();

            var dto = new ConversationDTO(conversation.ConversationId, participantUserIds, messages, lastRead);

            foreach (var pamUserId in participantUserIds)
            {
                var user = await this.accountRepository.GetByPamUserIdAsync(pamUserId);
                if (user != null)
                {
                    dto.ParticipantDisplayNames[pamUserId] = user.DisplayName;
                }
            }

            return dto;
        }

        private static MessageDataTransferObject MapEntityToDto(Message message)
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
                RequestId: message is RentalRequestMessage rrm2
                    ? ResolveRentalRequestId(rrm2, defaultMissingIdentifier)
                    : defaultMissingIdentifier,
                PaymentId: message is CashAgreementMessage cam2 ? cam2.CashPaymentId : defaultMissingIdentifier,
                RentalId: message is RentalRequestMessage rrm3
                    ? ResolveRentalId(rrm3, defaultMissingIdentifier)
                    : defaultMissingIdentifier);
        }

        private static int ResolveRentalRequestId(RentalRequestMessage rentalMessage, int missingId)
        {
            string content = rentalMessage.RequestContent ?? rentalMessage.MessageContentAsString ?? string.Empty;
            int requestId = RentalRequestMessageHelper.TryParseRequestIdFromContent(content);
            return requestId > 0 ? requestId : missingId;
        }

        private static int ResolveRentalId(RentalRequestMessage rentalMessage, int missingId)
        {
            string content = rentalMessage.RequestContent ?? rentalMessage.MessageContentAsString ?? string.Empty;
            int rentalId = RentalRequestMessageHelper.ResolveRentalId(rentalMessage.RentalRequestId ?? missingId, content);
            return rentalId > 0 ? rentalId : missingId;
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
                    RentalRequestId = dto.RentalId > 0 ? dto.RentalId : null,
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
