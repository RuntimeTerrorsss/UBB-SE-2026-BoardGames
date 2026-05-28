// <copyright file="ChatsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly IChatProxyService conversationProxyService;
        private readonly IAccountProxyService accountProxyService;

        public ChatsController(
            IConversationService conversationService,
            IUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            this.conversationService = conversationService;
            this.userRepository = userRepository;
            this.environment = environment;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ViewBag.IsLoggedIn = this.User?.Identity?.IsAuthenticated == true;
            this.ViewBag.CurrentUsername = this.User?.Identity?.Name;
            this.ViewBag.CurrentDisplayName = this.User?.GetDisplayName();
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.RedirectToAction("Login", "Auth");
            }

            int userId = this.User.GetPamUserId() ?? -1;
            this.conversationService.Initialize(userId);

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
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.RedirectToAction("Login", "Auth");
            }

            int currentUserId = this.User.GetPamUserId() ?? -1;

            if (currentUserId == ownerUserId)
            {
                return this.RedirectToAction("Index");
            }

            this.conversationService.Initialize(currentUserId);
            int conversationId = await this.conversationService.FindOrCreateConversationBetweenUsers(
                currentUserId, ownerUserId);

            return this.RedirectToAction("Index", new { openConversationId = conversationId });
        }

        [HttpGet]
        public async Task<IActionResult> GetChat(int conversationId)
        {
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.Unauthorized();
            }

            int currentUserId = this.User.GetPamUserId() ?? -1;
            this.conversationService.Initialize(currentUserId);

            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation is null)
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
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return this.BadRequest();
            }

            int senderId = this.User.GetPamUserId() ?? -1;
            this.conversationService.Initialize(senderId);

            var receiver = await this.GetReceiverParticipantAsync(conversationId, senderId);
            if (receiver == null)
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
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.Unauthorized();
            }

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

            int senderId = this.User.GetPamUserId() ?? -1;
            this.conversationService.Initialize(senderId);

            var receiver = await this.GetReceiverParticipantAsync(conversationId, senderId);
            if (receiver == null)
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
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.Unauthorized();
            }

            int userId = this.User.GetPamUserId() ?? -1;
            this.conversationService.Initialize(userId);

            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
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

            var updated = message with
            {
                IsAccepted = accepted,
                IsResolved = !accepted,
            };

            await this.conversationProxyService.UpdateMessageAsync(updated);
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CancelRentalRequest(int messageId, int conversationId)
        {
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.Unauthorized();
            }

            int userId = this.User.GetPamUserId() ?? -1;
            this.conversationService.Initialize(userId);

            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message is null || message.Type != MessageType.MessageRentalRequest)
            {
                return this.NotFound();
            }

            int currentPamUserId = await this.GetCurrentPamUserIdAsync();
            if (message.SenderId != currentPamUserId)
            {
                return this.BadRequest("Only the person who sent the request can cancel it.");
            }

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
            return otherPamUserId.HasValue ? $"User {otherPamUserId.Value}" : "Unknown user";
        }

        private int? GetOtherParticipantPamUserId(ConversationDTO conversation, int currentPamUserId)
        {
            return conversation.ParticipantUserIds
                .FirstOrDefault(participantId => participantId != currentPamUserId);
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
