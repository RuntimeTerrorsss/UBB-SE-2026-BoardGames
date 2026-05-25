// <copyright file="ConversationController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationRepository conversationRepository;

        public ConversationController(IConversationRepository conversationRepository)
        {
            this.conversationRepository = conversationRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Conversation>>> GetConversationsForUser(int userId)
        {
            var conversations = await this.conversationRepository.GetConversationsForUser(userId);
            return Ok(conversations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> GetConversationById(int id)
        {
            try
            {
                var conversation = await this.conversationRepository.GetConversationById(id);
                return Ok(conversation);
            }
            catch (InvalidOperationException)
            {
                return this.NotFound();
            }
        }

        [HttpGet("{id}/participants")]
        public async Task<ActionResult<IReadOnlyList<int>>> GetParticipantUserIds(int id)
        {
            var userIds = await this.conversationRepository.GetParticipantUserIds(id);
            return Ok(userIds);
        }

        public record CreateConversationRequest(int SenderId, int ReceiverId);

        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            if (request.SenderId <= 0 || request.ReceiverId <= 0 || request.SenderId == request.ReceiverId)
            {
                return this.BadRequest("Invalid conversation participants.");
            }

            int conversationId = await this.conversationRepository.CreateConversation(request.SenderId, request.ReceiverId);
            var created = await this.conversationRepository.GetConversationById(conversationId);
            return CreatedAtAction(nameof(this.GetConversationById), new { id = conversationId }, created);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] MessageDto messageDto)
        {
            var message = this.MessageDtoToEntity(messageDto);
            message.MessageId = 0;
            var persisted = await this.conversationRepository.HandleNewMessage(message);
            return this.Ok(EntityToMessageDto(persisted));
        }

        [HttpPut("messages")]
        public async Task<ActionResult<MessageDto>> UpdateMessage([FromBody] MessageDto messageDto)
        {
            var updated = await this.conversationRepository.HandleMessageUpdate(this.MessageDtoToEntity(messageDto));
            if (updated is null)
            {
                return this.NotFound();
            }

            return this.Ok(EntityToMessageDto(updated));
        }

        [HttpPost("readreceipt")]
        public async Task<ActionResult> SendReadReceipt([FromBody] ReadReceiptDto readReceipt)
        {
            var dto = new ReadReceiptDTO(
                readReceipt.ConversationId,
                readReceipt.ReaderId,
                readReceipt.ReceiverId,
                readReceipt.ReceiptTimeStamp);
            await this.conversationRepository.HandleReadReceipt(dto);
            return this.NoContent();
        }

        [HttpPost("rental/finalize/{messageId}")]
        public async Task<ActionResult<MessageDto>> FinalizeRentalRequest(int messageId)
        {
            var updated = await this.conversationRepository.HandleRentalRequestFinalization(messageId);
            if (updated is null)
            {
                return this.NotFound();
            }

            return this.Ok(EntityToMessageDto(updated));
        }

        [HttpPost("cash/{parentMessageId}/{paymentId}")]
        public async Task<ActionResult<MessageDto>> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var created = await this.conversationRepository.CreateCashAgreementMessage(parentMessageId, paymentId);
            if (created is null)
            {
                return this.NotFound();
            }

            return this.Ok(EntityToMessageDto(created));
        }

        private Message MessageDtoToEntity(MessageDto dto)
        {
            Message message = dto.Type switch
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

            return message;
        }

        private MessageDto EntityToMessageDto(Message message)
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

            return new MessageDto(
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

        public record MessageDto(
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

        public enum MessageType
        {
            MessageSystem,
            MessageText,
            MessageImage,
            MessageRentalRequest,
            MessageCashAgreement,
        }

        public record ReadReceiptDto(int ConversationId, int ReaderId, int ReceiverId, DateTime ReceiptTimeStamp);
    }
}
