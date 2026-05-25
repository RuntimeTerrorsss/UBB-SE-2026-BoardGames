// <copyright file="ChatsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Models.Chats;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class ChatsController : BaseController
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly IConversationService conversationService;
        private readonly IUserRepository userRepository;
        private readonly IWebHostEnvironment environment;

        public ChatsController(
            IConversationService conversationService,
            IUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            this.conversationService = conversationService;
            this.userRepository = userRepository;
            this.environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            int userId = this.CurrentUserId ?? -1;
            this.conversationService.Initialize(userId);

            var conversations = await this.conversationService.FetchConversations();
            var items = new List<ConversationListItemViewModel>();

            foreach (var conversation in conversations)
            {
                var otherUser = await GetOtherParticipantUserAsync(conversation, userId);
                var lastMessage = conversation.MessageList
                    .OrderByDescending(message => message.SentAt)
                    .FirstOrDefault();

                items.Add(new ConversationListItemViewModel
                {
                    ConversationId = conversation.Id,
                    OtherUserName = otherUser != null
                        ? FormatDisplayName(otherUser)
                        : await this.conversationService.GetOtherUserNameByConversationDTO(conversation),
                    OtherUserAvatarUrl = MediaUrlHelper.ResolveUserImageUrl(otherUser?.AvatarUrl),
                    LastMessagePreview = lastMessage?.GetChatMessagePreview() ?? "No messages yet",
                });
            }

            return this.View(items);
        }

        [HttpGet]
        public async Task<IActionResult> StartChatWithOwner(int ownerUserId)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            int currentUserId = this.CurrentUserId ?? -1;

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
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            int currentUserId = this.CurrentUserId ?? -1;
            this.conversationService.Initialize(currentUserId);

            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation == null)
            {
                return this.NotFound();
            }

            var otherUser = await GetOtherParticipantUserAsync(conversation, currentUserId);

            this.ViewBag.CurrentUserId = currentUserId;
            this.ViewBag.OtherUserName = otherUser != null
                ? FormatDisplayName(otherUser)
                : await this.conversationService.GetOtherUserNameByConversationDTO(conversation);
            this.ViewBag.OtherUserAvatarUrl = MediaUrlHelper.ResolveUserImageUrl(otherUser?.AvatarUrl);

            return PartialView("_ActiveChat", conversation);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return this.BadRequest();
            }

            int senderId = this.CurrentUserId ?? -1;
            this.conversationService.Initialize(senderId);

            var receiver = await this.GetReceiverParticipantAsync(conversationId, senderId);
            if (receiver == null)
            {
                return this.NotFound();
            }

            var dto = BuildMessageDto(
                conversationId,
                senderId,
                receiver.UserId,
                content.Trim(),
                MessageType.MessageText,
                string.Empty);

            await this.conversationService.SendMessage(dto);
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendImage(int conversationId, IFormFile image)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
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

            int senderId = this.CurrentUserId ?? -1;
            this.conversationService.Initialize(senderId);

            var receiver = await this.GetReceiverParticipantAsync(conversationId, senderId);
            if (receiver == null)
            {
                return this.NotFound();
            }

            string imagesDirectory = Path.Combine(this.environment.WebRootPath, "images");
            Directory.CreateDirectory(imagesDirectory);

            string storedFileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(imagesDirectory, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await image.CopyToAsync(stream);
            }

            var dto = BuildMessageDto(
                conversationId,
                senderId,
                receiver.UserId,
                "[Image]",
                MessageType.MessageImage,
                storedFileName);

            await this.conversationService.SendMessage(dto);
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResolveRentalRequest(int messageId, int conversationId, bool accepted)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return this.Unauthorized();
            }

            int userId = this.CurrentUserId ?? -1;
            this.conversationService.Initialize(userId);

            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.Type != MessageType.MessageRentalRequest)
            {
                return this.NotFound();
            }

            if (message.SenderId == userId)
            {
                return this.BadRequest("Only the game owner can accept or decline this request.");
            }

            var updated = message with
            {
                IsAccepted = accepted,
                IsResolved = !accepted,
            };

            await this.conversationService.UpdateMessage(updated);
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CancelRentalRequest(int messageId, int conversationId)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return this.Unauthorized();
            }

            int userId = this.CurrentUserId ?? -1;
            this.conversationService.Initialize(userId);

            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.Type != MessageType.MessageRentalRequest)
            {
                return this.NotFound();
            }

            if (message.SenderId != userId)
            {
                return this.BadRequest("Only the person who sent the request can cancel it.");
            }

            var updated = message with
            {
                IsAccepted = false,
                IsResolved = true,
            };

            await this.conversationService.UpdateMessage(updated);
            return this.Ok();
        }

        private async Task<User?> GetOtherParticipantUserAsync(ConversationDTO conversation, int currentUserId)
        {
            var otherParticipantIds = conversation.Participants
                .Select(participant => participant.UserId)
                .Where(participantId => participantId != currentUserId)
                .Distinct()
                .ToList();

            if (otherParticipantIds.Count == 0)
            {
                return null;
            }

            foreach (var otherUserId in otherParticipantIds)
            {
                var user = await this.userRepository.GetById(otherUserId);
                if (user is not null &&
                    !string.Equals(user.Username, "System", StringComparison.OrdinalIgnoreCase))
                {
                    return user;
                }
            }

            return await this.userRepository.GetById(otherParticipantIds.First());
        }

        private async Task<ConversationParticipant?> GetReceiverParticipantAsync(int conversationId, int senderId)
        {
            var conversations = await this.conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            return conversation?.Participants.FirstOrDefault(p => p.UserId != senderId);
        }

        private static MessageDTO BuildMessageDto(
            int conversationId,
            int senderId,
            int receiverId,
            string content,
            MessageType type,
            string imageUrl)
        {
            return new MessageDTO(
                Id: 0,
                ConversationId: conversationId,
                SenderId: senderId,
                ReceiverId: receiverId,
                SentAt: DateTime.Now,
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

        private static string FormatDisplayName(User user)
        {
            return !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName : user.Username;
        }
    }
}
