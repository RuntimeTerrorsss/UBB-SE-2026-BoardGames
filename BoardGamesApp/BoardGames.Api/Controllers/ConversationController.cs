// <copyright file="ConversationController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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
                var conversations = await this.conversationService.GetConversationsForUser(accountId);
                return this.Ok(conversations);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDTO>> GetConversationById(int id)
        {
            var conversation = await this.conversationService.GetConversationById(id);
            if (conversation is null)
            {
                return this.NotFound();
            }

            return this.Ok(conversation);
        }

        public record CreateConversationRequest(Guid SenderAccountId, Guid ReceiverAccountId);

        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                int conversationId = await this.conversationService.FindOrCreateConversation(request.SenderAccountId, request.ReceiverAccountId);
                var created = await this.conversationService.GetConversationById(conversationId);
                return this.CreatedAtAction(nameof(this.GetConversationById), new { id = conversationId }, created);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
        }

        [HttpPost("messages")]
        public async Task<ActionResult<MessageDataTransferObject>> SendMessage([FromBody] MessageDataTransferObject messageDto)
        {
            try
            {
                var persisted = await this.conversationService.SendMessage(messageDto);
                return this.Ok(persisted);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return this.StatusCode(403, ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
        }

        [HttpPut("messages")]
        public async Task<ActionResult<MessageDataTransferObject>> UpdateMessage([FromBody] MessageDataTransferObject messageDto)
        {
            var updated = await this.conversationService.UpdateMessage(messageDto);
            if (updated is null)
            {
                return this.NotFound();
            }

            return this.Ok(updated);
        }

        public record ReadReceiptRequest(int ConversationId, int ReaderId, int ReceiverId, DateTime ReceiptTimeStamp);

        [HttpPost("readreceipt")]
        public async Task<ActionResult> SendReadReceipt([FromBody] ReadReceiptRequest request)
        {
            var dto = new BoardGames.Data.Models.ReadReceiptDTO(
                request.ConversationId,
                request.ReaderId,
                request.ReceiverId,
                request.ReceiptTimeStamp);
            await this.conversationService.HandleReadReceipt(dto);
            return this.NoContent();
        }

        [HttpPost("rental/finalize/{requestId}")]
        public async Task<ActionResult> FinalizeRentalRequest(int requestId, [FromQuery] bool accepted = true)
        {
            await this.conversationService.FinalizeRentalRequestMessage(requestId, accepted);
            return this.NoContent();
        }

        [HttpPost("cash/{parentMessageId}/{paymentId}")]
        public async Task<ActionResult<MessageDataTransferObject>> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var created = await this.conversationService.CreateCashAgreementMessage(parentMessageId, paymentId);
            if (created is null)
            {
                return this.NotFound();
            }

            return this.Ok(created);
        }
    }
}
