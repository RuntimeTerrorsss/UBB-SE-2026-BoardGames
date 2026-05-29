//// <copyright file="ChatsControllerTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System.Security.Claims;
//using BoardGames.Shared.DTO;
//using BoardGames.Web.Controllers;
//using BoardGames.Web.Infrastructure;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Xunit;

//namespace BoardGames.Tests.Web
//{
//    public class ChatsControllerTests
//    {
//        private readonly Mock<IChatProxyService> chatProxy;
//        private readonly Mock<IAccountProxyService> accountProxy;
//        private readonly Mock<IRequestProxyService> requestProxy;

//        public ChatsControllerTests()
//        {
//            this.chatProxy = new Mock<IChatProxyService>();
//            this.accountProxy = new Mock<IAccountProxyService>();
//            this.requestProxy = new Mock<IRequestProxyService>();
//        }

//        private ChatsController CreateController(Guid accountId, int pamUserId)
//        {
//            var controller = new ChatsController(
//                this.chatProxy.Object,
//                this.accountProxy.Object,
//                this.requestProxy.Object);

//            var identity = new ClaimsIdentity(
//                new[]
//            {
//                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
//                new Claim("PamUserId", pamUserId.ToString()),
//            }, "Test");

//            controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext
//                {
//                    User = new ClaimsPrincipal(identity),
//                },
//            };

//            return controller;
//        }

//        [Fact]
//        public async Task ResolveRentalRequest_Accepted_CallsOfferGameAsync()
//        {
//            var accountId = Guid.NewGuid();
//            int ownerPamUserId = 10;
//            int senderPamUserId = 20;
//            int requestId = 42;

//            var message = new MessageDataTransferObject(
//                Id: 1,
//                ConversationId: 100,
//                SenderId: senderPamUserId,
//                ReceiverId: ownerPamUserId,
//                SentAt: DateTime.UtcNow,
//                Content: "Rental request",
//                Type: MessageType.MessageRentalRequest,
//                ImageUrl: string.Empty,
//                IsResolved: false,
//                IsAccepted: false,
//                IsAcceptedByBuyer: false,
//                IsAcceptedBySeller: false,
//                RequestId: requestId,
//                PaymentId: -1);

//            var conversation = new ConversationDTO
//            {
//                Id = 100,
//                MessageList = new List<MessageDataTransferObject> { message },
//                ParticipantUserIds = new List<int> { ownerPamUserId, senderPamUserId },
//            };

//            this.chatProxy
//                .Setup(s => s.GetConversationByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(conversation);

//            this.chatProxy
//                .Setup(s => s.UpdateMessageAsync(It.IsAny<MessageDataTransferObject>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(message);

//            var controller = this.CreateController(accountId, ownerPamUserId);

//            var result = await controller.ResolveRentalRequest(1, 100, true);

//            Assert.IsType<OkResult>(result);
//            this.requestProxy.Verify(
//                s => s.OfferGameAsync(requestId, It.Is<RequestActionDTO>(a => a.AccountId == accountId), It.IsAny<CancellationToken>()),
//                Times.Once);
//            this.requestProxy.Verify(
//                s => s.DenyRequestAsync(It.IsAny<int>(), It.IsAny<RequestActionDTO>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task ResolveRentalRequest_Declined_CallsDenyRequestAsync()
//        {
//            var accountId = Guid.NewGuid();
//            int ownerPamUserId = 10;
//            int senderPamUserId = 20;
//            int requestId = 42;

//            var message = new MessageDataTransferObject(
//                Id: 1,
//                ConversationId: 100,
//                SenderId: senderPamUserId,
//                ReceiverId: ownerPamUserId,
//                SentAt: DateTime.UtcNow,
//                Content: "Rental request",
//                Type: MessageType.MessageRentalRequest,
//                ImageUrl: string.Empty,
//                IsResolved: false,
//                IsAccepted: false,
//                IsAcceptedByBuyer: false,
//                IsAcceptedBySeller: false,
//                RequestId: requestId,
//                PaymentId: -1);

//            var conversation = new ConversationDTO
//            {
//                Id = 100,
//                MessageList = new List<MessageDataTransferObject> { message },
//                ParticipantUserIds = new List<int> { ownerPamUserId, senderPamUserId },
//            };

//            this.chatProxy
//                .Setup(s => s.GetConversationByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(conversation);

//            this.chatProxy
//                .Setup(s => s.UpdateMessageAsync(It.IsAny<MessageDataTransferObject>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(message);

//            var controller = this.CreateController(accountId, ownerPamUserId);

//            var result = await controller.ResolveRentalRequest(1, 100, false);

//            Assert.IsType<OkResult>(result);
//            this.requestProxy.Verify(
//                s => s.DenyRequestAsync(requestId, It.Is<RequestActionDTO>(a => a.AccountId == accountId), It.IsAny<CancellationToken>()),
//                Times.Once);
//            this.requestProxy.Verify(
//                s => s.OfferGameAsync(It.IsAny<int>(), It.IsAny<RequestActionDTO>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task ResolveRentalRequest_SenderCantResolve_ReturnsBadRequest()
//        {
//            var accountId = Guid.NewGuid();
//            int senderPamUserId = 20;

//            var message = new MessageDataTransferObject(
//                Id: 1,
//                ConversationId: 100,
//                SenderId: senderPamUserId,
//                ReceiverId: 10,
//                SentAt: DateTime.UtcNow,
//                Content: "Rental request",
//                Type: MessageType.MessageRentalRequest,
//                ImageUrl: string.Empty,
//                IsResolved: false,
//                IsAccepted: false,
//                IsAcceptedByBuyer: false,
//                IsAcceptedBySeller: false,
//                RequestId: 42,
//                PaymentId: -1);

//            var conversation = new ConversationDTO
//            {
//                Id = 100,
//                MessageList = new List<MessageDataTransferObject> { message },
//                ParticipantUserIds = new List<int> { 10, senderPamUserId },
//            };

//            this.chatProxy
//                .Setup(s => s.GetConversationByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(conversation);

//            var controller = this.CreateController(accountId, senderPamUserId);

//            var result = await controller.ResolveRentalRequest(1, 100, true);

//            Assert.IsType<BadRequestObjectResult>(result);
//            this.requestProxy.Verify(
//                s => s.OfferGameAsync(It.IsAny<int>(), It.IsAny<RequestActionDTO>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task CancelRentalRequest_CallsCancelRequestAsync()
//        {
//            var accountId = Guid.NewGuid();
//            int senderPamUserId = 20;
//            int requestId = 42;

//            var message = new MessageDataTransferObject(
//                Id: 1,
//                ConversationId: 100,
//                SenderId: senderPamUserId,
//                ReceiverId: 10,
//                SentAt: DateTime.UtcNow,
//                Content: "Rental request",
//                Type: MessageType.MessageRentalRequest,
//                ImageUrl: string.Empty,
//                IsResolved: false,
//                IsAccepted: false,
//                IsAcceptedByBuyer: false,
//                IsAcceptedBySeller: false,
//                RequestId: requestId,
//                PaymentId: -1);

//            var conversation = new ConversationDTO
//            {
//                Id = 100,
//                MessageList = new List<MessageDataTransferObject> { message },
//                ParticipantUserIds = new List<int> { 10, senderPamUserId },
//            };

//            this.chatProxy
//                .Setup(s => s.GetConversationByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(conversation);

//            this.chatProxy
//                .Setup(s => s.UpdateMessageAsync(It.IsAny<MessageDataTransferObject>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(message);

//            var controller = this.CreateController(accountId, senderPamUserId);

//            var result = await controller.CancelRentalRequest(1, 100);

//            Assert.IsType<OkResult>(result);
//            this.requestProxy.Verify(
//                s => s.CancelRequestAsync(requestId, It.Is<RequestActionDTO>(a => a.AccountId == accountId), It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task CancelRentalRequest_NonSenderCantCancel_ReturnsBadRequest()
//        {
//            var accountId = Guid.NewGuid();
//            int ownerPamUserId = 10;

//            var message = new MessageDataTransferObject(
//                Id: 1,
//                ConversationId: 100,
//                SenderId: 20,
//                ReceiverId: ownerPamUserId,
//                SentAt: DateTime.UtcNow,
//                Content: "Rental request",
//                Type: MessageType.MessageRentalRequest,
//                ImageUrl: string.Empty,
//                IsResolved: false,
//                IsAccepted: false,
//                IsAcceptedByBuyer: false,
//                IsAcceptedBySeller: false,
//                RequestId: 42,
//                PaymentId: -1);

//            var conversation = new ConversationDTO
//            {
//                Id = 100,
//                MessageList = new List<MessageDataTransferObject> { message },
//                ParticipantUserIds = new List<int> { ownerPamUserId, 20 },
//            };

//            this.chatProxy
//                .Setup(s => s.GetConversationByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(conversation);

//            var controller = this.CreateController(accountId, ownerPamUserId);

//            var result = await controller.CancelRentalRequest(1, 100);

//            Assert.IsType<BadRequestObjectResult>(result);
//            this.requestProxy.Verify(
//                s => s.CancelRequestAsync(It.IsAny<int>(), It.IsAny<RequestActionDTO>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task ResolveRentalRequest_MessageNotFound_ReturnsNotFound()
//        {
//            var accountId = Guid.NewGuid();

//            var conversation = new ConversationDTO
//            {
//                Id = 100,
//                MessageList = new List<MessageDataTransferObject>(),
//                ParticipantUserIds = new List<int> { 10, 20 },
//            };

//            this.chatProxy
//                .Setup(s => s.GetConversationByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(conversation);

//            var controller = this.CreateController(accountId, 10);

//            var result = await controller.ResolveRentalRequest(999, 100, true);

//            Assert.IsType<NotFoundResult>(result);
//        }
//    }
//}
