using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Web.Helpers;
using BoardGames.Web.Models.Chats;
using BoardGames.Data;
using BoardGames.Data.Enums;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class ChatsController : BaseController
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly IConversationService _conversationService;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _environment;

        public ChatsController(
            IConversationService conversationService,
            IUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            _conversationService = conversationService;
            _userRepository = userRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int userId = CurrentUserId ?? -1;
            _conversationService.Initialize(userId);

            var conversations = await _conversationService.FetchConversations();
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
                        : await _conversationService.GetOtherUserNameByConversationDTO(conversation),
                    OtherUserAvatarUrl = MediaUrlHelper.ResolveUserImageUrl(otherUser?.AvatarUrl),
                    LastMessagePreview = lastMessage?.GetChatMessagePreview() ?? "No messages yet",
                });
            }

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> StartChatWithOwner(int ownerUserId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int currentUserId = CurrentUserId ?? -1;

            if (currentUserId == ownerUserId)
            {
                return RedirectToAction("Index");
            }

            _conversationService.Initialize(currentUserId);
            int conversationId = await _conversationService.FindOrCreateConversationBetweenUsers(
                currentUserId, ownerUserId);

            return RedirectToAction("Index", new { openConversationId = conversationId });
        }

        [HttpGet]
        public async Task<IActionResult> GetChat(int conversationId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int currentUserId = CurrentUserId ?? -1;
            _conversationService.Initialize(currentUserId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation == null) return NotFound();

            var otherUser = await GetOtherParticipantUserAsync(conversation, currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserName = otherUser != null
                ? FormatDisplayName(otherUser)
                : await _conversationService.GetOtherUserNameByConversationDTO(conversation);
            ViewBag.OtherUserAvatarUrl = MediaUrlHelper.ResolveUserImageUrl(otherUser?.AvatarUrl);

            return PartialView("_ActiveChat", conversation);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest();
            }

            int senderId = CurrentUserId ?? -1;
            _conversationService.Initialize(senderId);

            var receiver = await GetReceiverParticipantAsync(conversationId, senderId);
            if (receiver == null) return NotFound();

            var dto = BuildMessageDto(
                conversationId,
                senderId,
                receiver.UserId,
                content.Trim(),
                MessageType.MessageText,
                string.Empty);

            await _conversationService.SendMessage(dto);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendImage(int conversationId, IFormFile image)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided.");
            }

            if (image.Length > MaxImageBytes)
            {
                return BadRequest("Image must be 5 MB or smaller.");
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return BadRequest("Only JPG, PNG, GIF, and WebP images are allowed.");
            }

            int senderId = CurrentUserId ?? -1;
            _conversationService.Initialize(senderId);

            var receiver = await GetReceiverParticipantAsync(conversationId, senderId);
            if (receiver == null) return NotFound();

            string imagesDirectory = Path.Combine(_environment.WebRootPath, "images");
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

            await _conversationService.SendMessage(dto);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResolveRentalRequest(int messageId, int conversationId, bool accepted)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            int userId = CurrentUserId ?? -1;
            _conversationService.Initialize(userId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.Type != MessageType.MessageRentalRequest)
            {
                return NotFound();
            }

            if (message.SenderId == userId)
            {
                return BadRequest("Only the game owner can accept or decline this request.");
            }

            var updated = message with
            {
                IsAccepted = accepted,
                IsResolved = !accepted,
            };

            await _conversationService.UpdateMessage(updated);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CancelRentalRequest(int messageId, int conversationId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            int userId = CurrentUserId ?? -1;
            _conversationService.Initialize(userId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.Type != MessageType.MessageRentalRequest)
            {
                return NotFound();
            }

            if (message.SenderId != userId)
            {
                return BadRequest("Only the person who sent the request can cancel it.");
            }

            var updated = message with
            {
                IsAccepted = false,
                IsResolved = true,
            };

            await _conversationService.UpdateMessage(updated);
            return Ok();
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
                var user = await _userRepository.GetById(otherUserId);
                if (user is not null &&
                    !string.Equals(user.Username, "System", StringComparison.OrdinalIgnoreCase))
                {
                    return user;
                }
            }

            return await _userRepository.GetById(otherParticipantIds.First());
        }

        private async Task<ConversationParticipant?> GetReceiverParticipantAsync(int conversationId, int senderId)
        {
            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            return conversation?.Participants.FirstOrDefault(p => p.UserId != senderId);
        }

        private static MessageDataTransferObject BuildMessageDto(
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
