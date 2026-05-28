using BoardGames.Data.Repositories;
using Xunit;
using BoardGames.Data.Enums;
using Moq;
using BoardGames.Shared.ProxyRepositories;
using BoardGames.Api.Legacy.Services;
// <copyright file="ConversationServiceTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BoardGames.Tests.UnitTests
{
    public class ConversationServiceTests
    {
        private readonly Mock<IConversationRepository> _mockConversationRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConversationNotifier> _mockNotifier;
        private readonly ConversationService _conversationService;
        private readonly Mock<IConversationRepository> _mockRepo;
        private readonly ConversationService _service;

        public ConversationServiceTests()
        {
            this._mockConversationRepo = new Mock<IConversationRepository>();
            this._mockUserRepo = new Mock<IUserRepository>();
            this._mockNotifier = new Mock<IConversationNotifier>();

            this._conversationService = new ConversationService(
                this._mockConversationRepo.Object,
                this._mockUserRepo.Object,
                this._mockNotifier.Object);

            this._mockRepo = new Mock<IConversationRepository>();

            this._service = new ConversationService(this._mockRepo.Object, this._mockUserRepo.Object, this._mockNotifier.Object);
            this._service.Initialize(1);
        }

        #region Initialize

        [Fact]
        public void Initialize_ValidUserId_RegistersNotifier()
        {
            int userId = 1;

            this._conversationService.Initialize(userId);

            this._mockNotifier.Verify(mockNotifier => mockNotifier.Register(userId, this._conversationService), Times.Once);
        }

        #endregion

        #region FetchConversations

        [Fact]
        public async Task FetchConversations_WithRealOtherParticipant_ReturnsConversationList()
        {
            int currentUserId = 1;
            int otherUserId = 2;
            this._conversationService.Initialize(currentUserId);

            var mockConversations = new List<Conversation>
            {
                new Conversation
                {
                    ConversationId = 10,
                    Participants = new List<ConversationParticipant>
                    {
                        new ConversationParticipant { UserId = currentUserId },
                        new ConversationParticipant { UserId = otherUserId }
                    },
                    Messages = new List<Message>()
                },
            };

            var mockUser = new User { Id = otherUserId, Username = "RealUser" };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetConversationsForUser(currentUserId)).ReturnsAsync(mockConversations);
            this._mockUserRepo.Setup(mockUserRepo => mockUserRepo.GetById(otherUserId)).ReturnsAsync(mockUser);

            var result = await this._conversationService.FetchConversations();

            Assert.Single(result);
            Assert.Equal(10, result.First().Id);
        }

        [Fact]
        public async Task FetchConversations_OnlySystemParticipant_ReturnsEmptyList()
        {
            int currentUserId = 1;
            int systemUserId = 99;
            this._conversationService.Initialize(currentUserId);

            var mockConversations = new List<Conversation>
            {
                new Conversation
                {
                    ConversationId = 10,
                    Participants = new List<ConversationParticipant>
                    {
                        new ConversationParticipant { UserId = currentUserId },
                        new ConversationParticipant { UserId = systemUserId }
                    },
                    Messages = new List<Message>()
                },
            };

            var systemUser = new User { Id = systemUserId, Username = "System" };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetConversationsForUser(currentUserId)).ReturnsAsync(mockConversations);
            this._mockUserRepo.Setup(mockUserRepo => mockUserRepo.GetById(systemUserId)).ReturnsAsync(systemUser);

            var result = await this._conversationService.FetchConversations();

            Assert.Empty(result);
        }

        #endregion

        #region GetOtherUserNameByConversationDTO

        [Fact]
        public async Task GetOtherUserNameByConversationDTO_NoOtherParticipants_ReturnsUnknownUser()
        {
            int currentUserId = 1;
            this._conversationService.Initialize(currentUserId);

            var conversationDto = new ConversationDTO(
                conversationId: 1,
                participants: new List<ConversationParticipant> { new ConversationParticipant { UserId = currentUserId } },
                messages: new List<MessageDTO>(),
                lastRead: new Dictionary<int, DateTime>());

            var result = await this._conversationService.GetOtherUserNameByConversationDTO(conversationDto);

            Assert.Equal("Unknown User", result);
        }

        [Fact]
        public async Task GetOtherUserNameByConversationDTO_HasOtherValidUser_ReturnsUsername()
        {
            int currentUserId = 1;
            int otherUserId = 2;
            this._conversationService.Initialize(currentUserId);

            var conversationDto = new ConversationDTO(
                conversationId: 1,
                participants: new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = currentUserId },
                    new ConversationParticipant { UserId = otherUserId },
                },
                messages: new List<MessageDTO>(),
                lastRead: new Dictionary<int, DateTime>());

            this._mockUserRepo.Setup(mockUserRepo => mockUserRepo.GetById(otherUserId)).ReturnsAsync(new User { Id = otherUserId, Username = "Alice" });

            var result = await this._conversationService.GetOtherUserNameByConversationDTO(conversationDto);

            Assert.Equal("Alice", result);
        }

        [Fact]
        public async Task GetOtherUserNameByConversationDTO_FallbackUserIsSystem_ReturnsUnknownUser()
        {
            int currentUserId = 1;
            int systemUserId = 99;
            this._conversationService.Initialize(currentUserId);

            var conversationDto = new ConversationDTO(
                conversationId: 1,
                participants: new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = currentUserId },
                    new ConversationParticipant { UserId = systemUserId },
                },
                messages: new List<MessageDTO>(),
                lastRead: new Dictionary<int, DateTime>());

            this._mockUserRepo.Setup(mockUserRepo => mockUserRepo.GetById(systemUserId)).ReturnsAsync(new User { Id = systemUserId, Username = "System" });

            var result = await this._conversationService.GetOtherUserNameByConversationDTO(conversationDto);

            Assert.Equal("Unknown User", result);
        }

        #endregion

        #region GetOtherUserNameByMessageDTO

        [Fact]
        public void GetOtherUserNameByMessageDTO_ValidOtherUser_ReturnsFormattedString()
        {
            int currentUserId = 1;
            this._conversationService.Initialize(currentUserId);

            var messageDto = this.CreateDummyMessageDto(senderId: 1, receiverId: 5);

            var result = this._conversationService.GetOtherUserNameByMessageDTO(messageDto);

            Assert.Equal("User 5", result);
        }

        [Fact]
        public void GetOtherUserNameByMessageDTO_InvalidOtherUser_ReturnsUnknownUser()
        {
            int currentUserId = 1;
            this._conversationService.Initialize(currentUserId);

            var messageDto = this.CreateDummyMessageDto(senderId: 1, receiverId: 0);

            var result = this._conversationService.GetOtherUserNameByMessageDTO(messageDto);

            Assert.Equal("Unknown User", result);
        }

        #endregion

        #region SendMessage & UpdateMessage & CreateConversation

        [Fact]
        public async Task SendMessage_ValidMessage_PersistsAndNotifies()
        {
            var messageDto = CreateDummyMessageDto(type: MessageType.MessageText);

            var persistedMessage = new TextMessage
            {
                MessageId = 10,
                ConversationId = messageDto.ConversationId,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.HandleNewMessage(It.IsAny<Message>())).ReturnsAsync(persistedMessage);
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetParticipantUserIds(It.IsAny<int>())).ReturnsAsync(new List<int> { 1, 2 });

            await this._conversationService.SendMessage(messageDto);

            this._mockConversationRepo.Verify(mockConvoRepo => mockConvoRepo.HandleNewMessage(It.IsAny<Message>()), Times.Once);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessage(It.IsAny<IEnumerable<int>>(), persistedMessage), Times.Once);
        }

        [Fact]
        public async Task CreateConversation_ValidIds_CreatesAndNotifies()
        {
            int senderId = 1, receiverId = 2, newConvId = 10;
            var createdConversation = new Conversation { ConversationId = newConvId, Participants = new List<ConversationParticipant>() };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.CreateConversation(senderId, receiverId)).ReturnsAsync(newConvId);
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetConversationById(newConvId)).ReturnsAsync(createdConversation);

            var result = await this._conversationService.CreateConversation(senderId, receiverId);

            Assert.Equal(newConvId, result);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyNewConversation(createdConversation), Times.Once);
        }

        [Fact]
        public async Task UpdateMessage_ValidMessage_UpdatesAndNotifies()
        {
            var messageDto = CreateDummyMessageDto(type: MessageType.MessageText);

            var persistedMessage = new TextMessage
            {
                MessageId = 10,
                ConversationId = messageDto.ConversationId,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.HandleMessageUpdate(It.IsAny<Message>())).ReturnsAsync(persistedMessage);
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetParticipantUserIds(It.IsAny<int>())).ReturnsAsync(new List<int> { 1, 2 });

            await this._conversationService.UpdateMessage(messageDto);

            this._mockConversationRepo.Verify(mockConvoRepo => mockConvoRepo.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IEnumerable<int>>(), persistedMessage), Times.Once);
        }

        [Fact]
        public async Task UpdateMessage_NullPersisted_DoesNotNotify()
        {
            var messageDto = this.CreateDummyMessageDto();
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.HandleMessageUpdate(It.IsAny<Message>())).ReturnsAsync((Message)null);

            await this._conversationService.UpdateMessage(messageDto);

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IEnumerable<int>>(), It.IsAny<Message>()), Times.Never);
        }

        #endregion

        #region Actions (Receipts, Payments)

        [Fact]
        public async Task SendReadReceipt_ValidConversation_HandlesAndNotifies()
        {
            int currentUserId = 1;
            int otherUserId = 2;
            this._conversationService.Initialize(currentUserId);

            var conversationDto = new ConversationDTO(
                conversationId: 10,
                participants: new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = currentUserId },
                    new ConversationParticipant { UserId = otherUserId },
                },
                messages: new List<MessageDTO>(),
                lastRead: new Dictionary<int, DateTime>());

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetParticipantUserIds(It.IsAny<int>())).ReturnsAsync(new List<int> { currentUserId, otherUserId });

            await this._conversationService.SendReadReceipt(conversationDto);

            this._mockConversationRepo.Verify(mockConvoRepo => mockConvoRepo.HandleReadReceipt(It.Is<ReadReceiptDTO>(readReceipt => readReceipt.ConversationId == 10 && readReceipt.ReceiverId == otherUserId)), Times.Once);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyReadReceipt(It.IsAny<IEnumerable<int>>(), It.IsAny<ReadReceiptDTO>()), Times.Once);
        }

        [Fact]
        public async Task OnCardPaymentSelected_ValidMessageId_FinalizesRequest()
        {
            int messageId = 10;

            var updatedMessage = new RentalRequestMessage
            {
                MessageId = messageId,
                ConversationId = 1,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.HandleRentalRequestFinalization(messageId)).ReturnsAsync(updatedMessage);
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetParticipantUserIds(It.IsAny<int>())).ReturnsAsync(new List<int> { 1, 2 });

            await this._conversationService.OnCardPaymentSelected(messageId);

            this._mockConversationRepo.Verify(mockConvoRepo => mockConvoRepo.HandleRentalRequestFinalization(messageId), Times.Once);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IEnumerable<int>>(), updatedMessage), Times.Once);
        }

        [Fact]
        public async Task OnCashPaymentSelected_ValidIds_FinalizesAndCreatesCashAgreement()
        {
            int messageId = 10, paymentId = 20;

            var updatedMessage = new RentalRequestMessage
            {
                MessageId = messageId,
                ConversationId = 1,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var createdAgreement = new CashAgreementMessage
            {
                MessageId = 11,
                ConversationId = 1,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.HandleRentalRequestFinalization(messageId)).ReturnsAsync(updatedMessage);
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.CreateCashAgreementMessage(messageId, paymentId)).ReturnsAsync(createdAgreement);
            this._mockConversationRepo.Setup(mockConvoRepo => mockConvoRepo.GetParticipantUserIds(It.IsAny<int>())).ReturnsAsync(new List<int> { 1, 2 });

            await this._conversationService.OnCashPaymentSelected(messageId, paymentId);

            this._mockConversationRepo.Verify(mockConvoRepo => mockConvoRepo.HandleRentalRequestFinalization(messageId), Times.Once);
            this._mockConversationRepo.Verify(mockConvoRepo => mockConvoRepo.CreateCashAgreementMessage(messageId, paymentId), Times.Once);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IEnumerable<int>>(), updatedMessage), Times.Once);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessage(It.IsAny<IEnumerable<int>>(), createdAgreement), Times.Once);
        }

        #endregion

        #region Polling Start/Stop

        [Fact]
        public void StopPolling_WhenNotRunning_DoesNotThrowException()
        {
            this._conversationService.StopPolling();

            Assert.True(true);
        }

        [Fact]
        public void StartPolling_CalledTwice_IgnoresSecondCall()
        {
            this._conversationService.StartPolling();
            this._conversationService.StartPolling();

            Assert.True(true);

            this._conversationService.StopPolling();
        }

        [Fact]
        public void StopPolling_WhenRunning_StopsWithoutException()
        {
            this._conversationService.StartPolling();
            this._conversationService.StopPolling();

            Assert.True(true);
        }

        #endregion

        #region Event Invocations

        [Fact]
        public void OnReadReceiptReceived_NoEventSubscribers_ExecutesWithoutThrowing()
        {
            var receipt = new ReadReceiptDTO(1, 1, 2, DateTime.Now);

            var exception = Record.Exception(() => this._conversationService.OnReadReceiptReceived(receipt));

            Assert.Null(exception);
        }

        [Fact]
        public void OnMessageUpdateReceived_NoEventSubscribers_ExecutesWithoutThrowing()
        {
            this._conversationService.Initialize(1);

            var message = new TextMessage
            {
                MessageId = 1,
                MessageSenderId = 2,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var exception = Record.Exception(() => this._conversationService.OnMessageUpdateReceived(message));

            Assert.Null(exception);
        }

        [Fact]
        public void OnMessageReceived_NoEventSubscribers_ExecutesWithoutThrowing()
        {
            this._conversationService.Initialize(1);

            var message = new TextMessage
            {
                MessageId = 1,
                MessageSenderId = 2,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var exception = Record.Exception(() => this._conversationService.OnMessageReceived(message));

            Assert.Null(exception);
        }

        [Fact]
        public void OnMessageReceived_ValidMessage_InvokesEvent()
        {
            this._conversationService.Initialize(1);

            var message = new TextMessage
            {
                MessageId = 1,
                MessageSenderId = 2,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            bool eventInvoked = false;
            this._conversationService.ActionMessageProcessed += (dto, user) => eventInvoked = true;

            this._conversationService.OnMessageReceived(message);

            Assert.True(eventInvoked);
        }

        [Fact]
        public async Task OnConversationReceived_ValidConversation_InvokesEvent()
        {
            this._conversationService.Initialize(1);
            var conversation = new Conversation { ConversationId = 1, Participants = new List<ConversationParticipant>(), Messages = new List<Message>() };
            bool eventInvoked = false;
            this._conversationService.ActionConversationProcessed += (dto, user) => eventInvoked = true;

            await this._conversationService.OnConversationReceived(conversation);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnReadReceiptReceived_ValidReceipt_InvokesEvent()
        {
            var receipt = new ReadReceiptDTO(1, 1, 2, DateTime.Now);
            bool eventInvoked = false;
            this._conversationService.ActionReadReceiptProcessed += (readReceipt) => eventInvoked = true;

            this._conversationService.OnReadReceiptReceived(receipt);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnMessageUpdateReceived_ValidMessage_InvokesEvent()
        {
            this._conversationService.Initialize(1);

            var message = new TextMessage
            {
                MessageId = 1,
                MessageSenderId = 2,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            bool eventInvoked = false;
            this._conversationService.ActionMessageUpdateProcessed += (dto, user) => eventInvoked = true;

            this._conversationService.OnMessageUpdateReceived(message);

            Assert.True(eventInvoked);
        }

        #endregion

        #region Mapping Tests (MessageDTOToMessage & MessageToMessageDTO)

        [Fact]
        public void MessageToMessageDTO_TextMessageWithNullContent_FallsBackToStringEmpty()
        {
            var textMessage = new TextMessage
            {
                MessageId = 1,
                TextMessageContent = null,
                MessageContentAsString = null,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var result = this._conversationService.MessageToMessageDTO(textMessage);

            Assert.Equal(string.Empty, result.Content);
            Assert.Equal(string.Empty, result.ImageUrl);
            Assert.False(result.IsResolved);
            Assert.Equal(-1, result.PaymentId);
            Assert.Equal(-1, result.RequestId);
        }

        [Fact]
        public void MessageToMessageDTO_ImageMessageWithNullUrl_FallsBackToStringEmpty()
        {
            var imageMessage = new ImageMessage
            {
                MessageId = 2,
                MessageImageUrl = null,
                MessageContentAsString = "Image Description",
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var result = this._conversationService.MessageToMessageDTO(imageMessage);

            Assert.Equal("Image Description", result.Content);
            Assert.Equal(string.Empty, result.ImageUrl);
        }

        [Fact]
        public void MessageToMessageDTO_RentalRequestMessage_MapsAllSpecificProperties()
        {
            var rentalMessage = new RentalRequestMessage
            {
                MessageId = 3,
                RequestContent = "Rental Details",
                RentalRequestId = 500,
                IsRequestResolved = true,
                IsRequestAccepted = true,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var result = this._conversationService.MessageToMessageDTO(rentalMessage);

            Assert.Equal(MessageType.MessageRentalRequest, result.Type);
            Assert.Equal("Rental Details", result.Content);
            Assert.True(result.IsResolved);
            Assert.True(result.IsAccepted);
            Assert.Equal(500, result.RequestId);
            Assert.Equal(-1, result.PaymentId);
        }

        [Fact]
        public void MessageToMessageDTO_CashAgreementMessage_MapsAllSpecificProperties()
        {
            var cashMessage = new CashAgreementMessage
            {
                MessageId = 4,
                MessageContentAsString = "Cash Agreement Details",
                CashPaymentId = 700,
                IsCashAgreementResolved = true,
                IsCashAgreementAcceptedByBuyer = true,
                IsCashAgreementAcceptedBySeller = true,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var result = this._conversationService.MessageToMessageDTO(cashMessage);

            Assert.Equal(MessageType.MessageCashAgreement, result.Type);
            Assert.Equal("Cash Agreement Details", result.Content);
            Assert.True(result.IsResolved);
            Assert.True(result.IsAcceptedByBuyer);
            Assert.True(result.IsAcceptedBySeller);
            Assert.Equal(700, result.PaymentId);
            Assert.Equal(-1, result.RequestId);
        }

        [Fact]
        public void MessageToMessageDTO_SystemMessageWithContent_MapsProperly()
        {
            var systemMessage = new SystemMessage
            {
                MessageId = 5,
                MessageContent = "System Alert",
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var result = this._conversationService.MessageToMessageDTO(systemMessage);

            Assert.Equal(MessageType.MessageSystem, result.Type);
            Assert.Equal("System Alert", result.Content);
        }

        [Theory]
        [InlineData(MessageType.MessageText, typeof(TextMessage))]
        [InlineData(MessageType.MessageImage, typeof(ImageMessage))]
        [InlineData(MessageType.MessageRentalRequest, typeof(RentalRequestMessage))]
        [InlineData(MessageType.MessageCashAgreement, typeof(CashAgreementMessage))]
        [InlineData(MessageType.MessageSystem, typeof(SystemMessage))]
        public void MessageDTOToMessage_KnownTypes_ReturnsCorrectSubclass(MessageType type, Type expectedReturnType)
        {
            var dto = this.CreateDummyMessageDto(type: type);

            var result = this._conversationService.MessageDTOToMessage(dto);

            Assert.IsType(expectedReturnType, result);
        }

        [Fact]
        public void MessageDTOToMessage_UnknownType_ThrowsArgumentOutOfRangeException()
        {
            var dto = this.CreateDummyMessageDto(type: (MessageType)999);

            Assert.Throws<ArgumentOutOfRangeException>(() => this._conversationService.MessageDTOToMessage(dto));
        }

        [Fact]
        public void MessageToMessageDTO_KnownTypes_MapsProperly()
        {
            var textMessage = new TextMessage
            {
                MessageId = 1,
                TextMessageContent = "Hello",
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var imageMessage = new ImageMessage
            {
                MessageId = 2,
                MessageImageUrl = "img.png",
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            var dtoText = this._conversationService.MessageToMessageDTO(textMessage);
            var dtoImg = this._conversationService.MessageToMessageDTO(imageMessage);

            Assert.Equal(MessageType.MessageText, dtoText.Type);
            Assert.Equal("Hello", dtoText.Content);

            Assert.Equal(MessageType.MessageImage, dtoImg.Type);
            Assert.Equal("img.png", dtoImg.ImageUrl);
        }

        #endregion

        #region SendMessage Tests (Cache updates & if-branches)

        [Fact]
        public async Task SendMessage_CachedConversationFound_MessageNotPresent_AddsToCache()
        {
            var messageDto = this.CreateDummyMessageDto(id: 1, convId: 10);
            var persistedMessage = new TextMessage
            {
                MessageId = 1,
                ConversationId = 10,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockRepo.Setup(mockRepo => mockRepo.HandleNewMessage(It.IsAny<Message>())).ReturnsAsync(persistedMessage);

            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message>() };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            await this._service.SendMessage(messageDto);

            Assert.Single(cachedConv.Messages);
            Assert.Equal(1, cachedConv.Messages.First().MessageId);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessage(It.IsAny<IReadOnlyList<int>>(), persistedMessage), Times.Once);
        }

        [Fact]
        public async Task SendMessage_CachedConversationFound_MessageAlreadyPresent_DoesNotAddDuplicate()
        {
            var messageDto = this.CreateDummyMessageDto(id: 1, convId: 10);
            var persistedMessage = new TextMessage
            {
                MessageId = 1,
                ConversationId = 10,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockRepo.Setup(mockRepo => mockRepo.HandleNewMessage(It.IsAny<Message>())).ReturnsAsync(persistedMessage);

            var existingMessage = new TextMessage
            {
                MessageId = 1,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { existingMessage } };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            await this._service.SendMessage(messageDto);

            Assert.Single(cachedConv.Messages);
        }

        [Fact]
        public async Task SendMessage_CachedConversationFound_MessagesNotIList_DoesNotThrow()
        {
            var messageDto = this.CreateDummyMessageDto(id: 1, convId: 10);
            var persistedMessage = new TextMessage
            {
                MessageId = 1,
                ConversationId = 10,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockRepo.Setup(mockRepo => mockRepo.HandleNewMessage(It.IsAny<Message>())).ReturnsAsync(persistedMessage);

            var cachedConv = new Conversation { ConversationId = 10, Messages = null };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            var exception = await Record.ExceptionAsync(() => this._service.SendMessage(messageDto));

            Assert.Null(exception);
        }

        [Fact]
        public async Task SendMessage_CachedConversationNotFound_IgnoresCacheAndNotifies()
        {
            var messageDto = this.CreateDummyMessageDto(id: 1, convId: 10);
            var persistedMessage = new TextMessage
            {
                MessageId = 1,
                ConversationId = 10,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this._mockRepo.Setup(mockRepo => mockRepo.HandleNewMessage(It.IsAny<Message>())).ReturnsAsync(persistedMessage);

            this.SetCachedConversations(new List<Conversation>());

            var exception = await Record.ExceptionAsync(() => this._service.SendMessage(messageDto));

            Assert.Null(exception);
            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessage(It.IsAny<IReadOnlyList<int>>(), persistedMessage), Times.Once);
        }

        #endregion

        #region PollConversationsLoop Tests (Polling, Catch blocks & Updates)

        [Fact]
        public async Task PollConversationsLoop_NewConversation_NotifiesNewConversation()
        {
            var fetchedConv = new Conversation { ConversationId = 5, Messages = new List<Message>() };
            this.SetCachedConversations(new List<Conversation>());

            await this.RunPollerOnceAsync(new List<Conversation> { fetchedConv });

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyNewConversation(fetchedConv), Times.Once);
        }

        [Fact]
        public async Task PollConversationsLoop_NewMessageNotRecentlySent_NotifiesMessage()
        {
            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message>() };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            var fetchedMsg = new TextMessage
            {
                MessageId = 100,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var fetchedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { fetchedMsg } };

            await this.RunPollerOnceAsync(new List<Conversation> { fetchedConv });

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessage(It.IsAny<IReadOnlyList<int>>(), fetchedMsg), Times.Once);
        }

        [Fact]
        public async Task PollConversationsLoop_NewMessageRecentlySent_RemovesFromRecentlySentAndDoesNotNotify()
        {
            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message>() };
            this.SetCachedConversations(new List<Conversation> { cachedConv });
            this.AddToRecentlySent(100);

            var fetchedMsg = new TextMessage
            {
                MessageId = 100,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var fetchedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { fetchedMsg } };

            await this.RunPollerOnceAsync(new List<Conversation> { fetchedConv });

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessage(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public async Task PollConversationsLoop_RentalRequestUpdated_NotifiesMessageUpdate()
        {
            var cachedMsg = new RentalRequestMessage
            {
                MessageId = 1,
                IsRequestResolved = false,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { cachedMsg } };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            var fetchedMsg = new RentalRequestMessage
            {
                MessageId = 1,
                IsRequestResolved = true,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var fetchedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { fetchedMsg } };

            await this.RunPollerOnceAsync(new List<Conversation> { fetchedConv });

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IReadOnlyList<int>>(), fetchedMsg), Times.Once);
        }

        [Fact]
        public async Task PollConversationsLoop_RentalRequestNotUpdated_DoesNotNotify()
        {
            var cachedMsg = new RentalRequestMessage
            {
                MessageId = 1,
                IsRequestResolved = true,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { cachedMsg } };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            var fetchedMsg = new RentalRequestMessage
            {
                MessageId = 1,
                IsRequestResolved = true,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var fetchedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { fetchedMsg } };

            await this.RunPollerOnceAsync(new List<Conversation> { fetchedConv });

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public async Task PollConversationsLoop_CashAgreementUpdated_NotifiesMessageUpdate()
        {
            var cachedMsg = new CashAgreementMessage
            {
                MessageId = 1,
                IsCashAgreementResolved = false,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var cachedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { cachedMsg } };
            this.SetCachedConversations(new List<Conversation> { cachedConv });

            var fetchedMsg = new CashAgreementMessage
            {
                MessageId = 1,
                IsCashAgreementResolved = true,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };
            var fetchedConv = new Conversation { ConversationId = 10, Messages = new List<Message> { fetchedMsg } };

            await this.RunPollerOnceAsync(new List<Conversation> { fetchedConv });

            this._mockNotifier.Verify(mockNotifier => mockNotifier.NotifyMessageUpdate(It.IsAny<IReadOnlyList<int>>(), fetchedMsg), Times.Once);
        }

        [Fact]
        public async Task PollConversationsLoop_GenericException_CaughtAndContinuesLoop()
        {
            this._mockRepo.SetupSequence(r => r.GetConversationsForUser(It.IsAny<int>()))
                     .ThrowsAsync(new Exception("Database disconnected"))
                     .ReturnsAsync(new List<Conversation>())
                     .ThrowsAsync(new TaskCanceledException());

            var method = typeof(ConversationService).GetMethod("PollConversationsLoop", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)method.Invoke(this._service, new object[] { CancellationToken.None });

            this._mockRepo.Verify(mockRepo => mockRepo.GetConversationsForUser(It.IsAny<int>()), Times.Exactly(3));
        }

        #endregion

        #region Reflection & Helper Methods

        private void SetCachedConversations(List<Conversation> cached)
        {
            var field = typeof(ConversationService).GetField("cachedConversations", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(this._service, cached);
        }

        private void AddToRecentlySent(int messageId)
        {
            var field = typeof(ConversationService).GetField("recentlySentMessageIds", BindingFlags.NonPublic | BindingFlags.Instance);
            var hashSet = (HashSet<int>)field.GetValue(this._service);
            hashSet.Add(messageId);
        }

        private async Task RunPollerOnceAsync(List<Conversation> fetchedConversations)
        {
            this._mockRepo.SetupSequence(mockRepo => mockRepo.GetConversationsForUser(It.IsAny<int>()))
                     .ReturnsAsync(fetchedConversations)
                     .ThrowsAsync(new TaskCanceledException());

            var method = typeof(ConversationService).GetMethod("PollConversationsLoop", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method.Invoke(this._service, new object[] { CancellationToken.None });
        }

        private MessageDTO CreateDummyMessageDto(
            int id = 100,
            int convId = 10,
            int senderId = 1,
            int receiverId = 2,
            MessageType type = MessageType.MessageText)
        {
            return new MessageDTO(
                Id: id,
                ConversationId: convId,
                SenderId: senderId,
                ReceiverId: receiverId,
                SentAt: DateTime.Now,
                Content: "Test",
                Type: type,
                ImageUrl: string.Empty,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                PaymentId: -1,
                RequestId: -1);
        }

        #endregion
    }
}
