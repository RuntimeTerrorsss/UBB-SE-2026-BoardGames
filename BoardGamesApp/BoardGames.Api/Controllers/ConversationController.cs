using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationApiService conversationService;

        public ConversationController(IConversationApiService conversationService)
        {
            this.conversationService = conversationService;
        }

        [HttpGet("user/{accountId:guid}")]
        public async Task<ActionResult<List<ConversationDTO>>> GetConversationsForUser(Guid accountId)
        {
            try
            {
                var conversations = await conversationService.GetConversationsForUser(accountId);
                return Ok(conversations);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDTO>> GetConversationById(int id)
        {
            var conversation = await conversationService.GetConversationById(id);
            if (conversation is null)
            {
                return NotFound();
            }

            return Ok(conversation);
        }

        public record CreateConversationRequest(Guid SenderAccountId, Guid ReceiverAccountId);

        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                int conversationId = await conversationService.FindOrCreateConversation(request.SenderAccountId, request.ReceiverAccountId);
                var created = await conversationService.GetConversationById(conversationId);
                return CreatedAtAction(nameof(GetConversationById), new { id = conversationId }, created);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("messages")]
        public async Task<ActionResult<MessageDTO>> SendMessage([FromBody] MessageDTO messageDto)
        {
            var persisted = await conversationService.SendMessage(messageDto);
            return Ok(persisted);
        }

        [HttpPut("messages")]
        public async Task<ActionResult<MessageDTO>> UpdateMessage([FromBody] MessageDTO messageDto)
        {
            var updated = await conversationService.UpdateMessage(messageDto);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        public record ReadReceiptRequest(int ConversationId, int ReaderId, int ReceiverId, DateTime ReceiptTimeStamp);

        [HttpPost("readreceipt")]
        public async Task<ActionResult> SendReadReceipt([FromBody] ReadReceiptRequest request)
        {
            var dto = new ReadReceiptDTO(
                request.ConversationId,
                request.ReaderId,
                request.ReceiverId,
                request.ReceiptTimeStamp);
            await conversationService.HandleReadReceipt(dto);
            return NoContent();
        }

        [HttpPost("rental/finalize/{requestId}")]
        public async Task<ActionResult> FinalizeRentalRequest(int requestId, [FromQuery] bool accepted = true)
        {
            await conversationService.FinalizeRentalRequestMessage(requestId, accepted);
            return NoContent();
        }

        [HttpPost("cash/{parentMessageId}/{paymentId}")]
        public async Task<ActionResult<MessageDTO>> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var created = await conversationService.CreateCashAgreementMessage(parentMessageId, paymentId);
            if (created is null)
            {
                return NotFound();
            }

            return Ok(created);
        }
    }
}
