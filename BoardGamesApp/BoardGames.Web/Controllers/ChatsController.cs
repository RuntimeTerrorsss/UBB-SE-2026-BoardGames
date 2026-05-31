// <copyright file="ChatsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly IChatProxyService conversationProxyService;
        private readonly IAccountProxyService accountProxyService;
        private readonly IRequestProxyService requestProxyService;

        public ChatsController(
            IChatProxyService conversationProxyService,
            IAccountProxyService accountProxyService,
            IRequestProxyService requestProxyService)
        {
            this.conversationProxyService = conversationProxyService ?? throw new ArgumentNullException(nameof(conversationProxyService));
            this.accountProxyService = accountProxyService ?? throw new ArgumentNullException(nameof(accountProxyService));
            this.requestProxyService = requestProxyService ?? throw new ArgumentNullException(nameof(requestProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Guid accountId = this.User.GetAccountId();
            int currentPamUserId = await this.GetCurrentPamUserIdAsync();

            var conversations = await this.conversationProxyService.GetConversationsForUserAsync(accountId);
            var items = new List<ConversationListItemViewModel>();

            foreach (var conversation in conversations)
            {
                var lastMessage = conversation.MessageList
                    .OrderByDescending(message => message.SentAt)
                    .FirstOrDefault();

                items.Add(new ConversationListItemViewModel
                {
                    ConversationId = conversation.Id,
                    OtherUserName = this.ResolveOtherUserName(conversation, currentPamUserId),
                    LastMessagePreview = lastMessage?.GetChatMessagePreview() ?? "No messages yet",
                });
            }

            return this.View(items);
        }

        [HttpGet]
        public async Task<IActionResult> StartChatWithOwner(int ownerUserId)
        {
            int currentPamUserId = await this.GetCurrentPamUserIdAsync();
            if (currentPamUserId == ownerUserId)
            {
                return this.RedirectToAction(nameof(this.Index));
            }

            Guid accountId = this.User.GetAccountId();
            var conversations = await this.conversationProxyService.GetConversationsForUserAsync(accountId);
            var existing = conversations.FirstOrDefault(c =>
                c.ParticipantUserIds.Contains(ownerUserId) && c.ParticipantUserIds.Contains(currentPamUserId));

            if (existing is not null)
            {
                return this.RedirectToAction(nameof(this.Index), new { openConversationId = existing.Id });
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetChat(int conversationId)
        {
            int currentPamUserId = await this.GetCurrentPamUserIdAsync();
            var conversation = await this.conversationProxyService.GetConversationByIdAsync(conversationId);

            if (conversation is null || !conversation.ParticipantUserIds.Contains(currentPamUserId))
            {
                return this.NotFound();
            }

            this.ViewBag.CurrentUserId = currentPamUserId;
            this.ViewBag.OtherUserName = this.ResolveOtherUserName(conversation, currentPamUserId);

            return this.PartialView("_ActiveChat", conversation);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return this.BadRequest();
            }

            int senderPamUserId = await this.GetCurrentPamUserIdAsync();
            var conversation = await this.conversationProxyService.GetConversationByIdAsync(conversationId);
            int? receiverPamUserId = conversation is null
                ? null
                : this.GetOtherParticipantPamUserId(conversation, senderPamUserId);

            if (!receiverPamUserId.HasValue)
            {
                return this.NotFound();
            }

            var message = this.BuildMessage(
                conversationId,
                senderPamUserId,
                receiverPamUserId.Value,
                content.Trim(),
                MessageType.MessageText,
                string.Empty);

            await this.conversationProxyService.SendMessageAsync(message);
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendImage(int conversationId, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return this.BadRequest("No image provided.");
            }

            if (image.Length > MaxImageBytes)
            {
                return this.BadRequest("Image must be 5 MB or smaller.");
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return this.BadRequest("Only JPG, PNG, GIF, and WebP images are allowed.");
            }

            int senderPamUserId = await this.GetCurrentPamUserIdAsync();
            var conversation = await this.conversationProxyService.GetConversationByIdAsync(conversationId);
            int? receiverPamUserId = conversation is null
                ? null
                : this.GetOtherParticipantPamUserId(conversation, senderPamUserId);

            if (!receiverPamUserId.HasValue)
            {
                return this.NotFound();
            }

            string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(imagesDirectory);

            string storedFileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(imagesDirectory, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await image.CopyToAsync(stream);
            }

            var message = this.BuildMessage(
                conversationId,
                senderPamUserId,
                receiverPamUserId.Value,
                "[Image]",
                MessageType.MessageImage,
                storedFileName);

            await this.conversationProxyService.SendMessageAsync(message);
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResolveRentalRequest(int messageId, int conversationId, bool accepted)
        {
            var conversation = await this.conversationProxyService.GetConversationByIdAsync(conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message is null || message.Type != MessageType.MessageRentalRequest)
            {
                return this.NotFound();
            }

            int currentPamUserId = await this.GetCurrentPamUserIdAsync();
            if (message.SenderId == currentPamUserId)
            {
                return this.BadRequest("Only the game owner can accept or decline this request.");
            }

            int requestId = BoardGames.Shared.Helpers.RentalRequestMessageHelper.ResolveRequestId(message.RequestId, message.Content);
            if (requestId <= 0)
            {
                return this.NotFound();
            }

            Guid ownerAccountId = this.User.GetAccountId();
            var actionBody = new RequestActionDTO { AccountId = ownerAccountId };

            if (accepted)
            {
                await this.requestProxyService.OfferGameAsync(requestId, actionBody);
            }
            else
            {
                await this.requestProxyService.DenyRequestAsync(requestId, actionBody);
            }

            var refreshed = await this.conversationProxyService.GetConversationByIdAsync(conversationId);
            return this.Ok(refreshed?.MessageList.FirstOrDefault(m => m.Id == messageId));
        }

        [HttpPost]
        public async Task<IActionResult> CancelRentalRequest(int messageId, int conversationId)
        {
            var conversation = await this.conversationProxyService.GetConversationByIdAsync(conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message is null || message.Type != MessageType.MessageRentalRequest)
            {
                return this.NotFound();
            }

            int requestId = BoardGames.Shared.Helpers.RentalRequestMessageHelper.ResolveRequestId(message.RequestId, message.Content);
            if (requestId <= 0)
            {
                return this.NotFound();
            }

            int currentPamUserId = await this.GetCurrentPamUserIdAsync();
            if (message.SenderId != currentPamUserId)
            {
                return this.BadRequest("Only the person who sent the request can cancel it.");
            }

            Guid renterAccountId = this.User.GetAccountId();
            await this.requestProxyService.CancelRequestAsync(requestId, new RequestActionDTO
            {
                AccountId = renterAccountId,
            });

            var updated = message with
            {
                IsAccepted = false,
                IsResolved = true,
            };

            await this.conversationProxyService.UpdateMessageAsync(updated);
            return this.Ok();
        }

        private async Task<int> GetCurrentPamUserIdAsync()
        {
            if (this.User.TryGetPamUserId(out int pamUserId))
            {
                return pamUserId;
            }

            Guid accountId = this.User.GetAccountId();
            AccountProfileDTO profile = await this.accountProxyService.GetProfileAsync(accountId);
            if (profile.PamUserId is null or <= 0)
            {
                throw new InvalidOperationException("Current account does not have a valid PAM user id.");
            }

            return profile.PamUserId.Value;
        }

        private string ResolveOtherUserName(ConversationDTO conversation, int currentPamUserId)
        {
            int? otherPamUserId = this.GetOtherParticipantPamUserId(conversation, currentPamUserId);
            if (!otherPamUserId.HasValue)
            {
                return "Unknown user";
            }

            conversation.ParticipantDisplayNames.TryGetValue(otherPamUserId.Value, out string? displayName);
            return displayName ?? $"User {otherPamUserId.Value}";
        }

        private int? GetOtherParticipantPamUserId(ConversationDTO conversation, int currentPamUserId)
        {
            foreach (int participantId in conversation.ParticipantUserIds)
            {
                if (participantId != currentPamUserId)
                {
                    return participantId;
                }
            }

            return null;
        }

        private MessageDataTransferObject BuildMessage(
            int conversationId,
            int senderId,
            int receiverId,
            string content,
            MessageType type,
            string imageUrl)
        {
            return new MessageDataTransferObject(
                Id: 0,
                ConversationId: conversationId,
                SenderId: senderId,
                ReceiverId: receiverId,
                SentAt: DateTime.UtcNow,
                Content: content,
                Type: type,
                ImageUrl: imageUrl,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                RequestId: -1,
                PaymentId: -1);
        }
    }
}
