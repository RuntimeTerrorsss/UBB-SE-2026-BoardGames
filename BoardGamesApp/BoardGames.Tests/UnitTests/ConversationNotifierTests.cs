using BoardGames.Api.Legacy.Services;
using Xunit;
// <copyright file="ConversationNotifierTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace BoardGames.Tests.UnitTests
{
    public class ConversationNotifierTests
    {
        private readonly ConversationNotifier _notifier;

        public ConversationNotifierTests()
        {
            this._notifier = new ConversationNotifier();
        }

        [Fact]
        public void NotifyMessage_RegisteredUser_ReceivesNotification()
        {
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object;

            this._notifier.Register(userId, mockObserver.Object);

            this._notifier.NotifyMessage(new[] { userId }, message);

            mockObserver.Verify(mockObserver => mockObserver.OnMessageReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyMessage_UnregisteredUser_DoesNotReceiveNotification()
        {
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object;

            this._notifier.Register(userId, mockObserver.Object);
            this._notifier.Unregister(userId);

            this._notifier.NotifyMessage(new[] { userId }, message);

            mockObserver.Verify(mockObserver => mockObserver.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public void NotifyMessage_MultipleRegisteredUsers_OnlySpecifiedUsersReceiveNotification()
        {
            var mockObserver1 = new Mock<IConversationService>();
            var mockObserver2 = new Mock<IConversationService>();
            var mockObserver3 = new Mock<IConversationService>();
            var message = new Mock<Message>().Object;

            this._notifier.Register(1, mockObserver1.Object);
            this._notifier.Register(2, mockObserver2.Object);
            this._notifier.Register(3, mockObserver3.Object);

            this._notifier.NotifyMessage(new[] { 1, 3 }, message);

            mockObserver1.Verify(mockObserver => mockObserver.OnMessageReceived(message), Times.Once);
            mockObserver2.Verify(mockObserver => mockObserver.OnMessageReceived(It.IsAny<Message>()), Times.Never);
            mockObserver3.Verify(mockObserver => mockObserver.OnMessageReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyMessage_DuplicateUserIdsInList_ReceivesNotificationOnlyOnce()
        {
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object;

            this._notifier.Register(userId, mockObserver.Object);

            this._notifier.NotifyMessage(new[] { userId, userId, userId }, message);

            mockObserver.Verify(mockObserver => mockObserver.OnMessageReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyMessageUpdate_RegisteredUsers_ReceiveUpdateNotification()
        {
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object;

            this._notifier.Register(userId, mockObserver.Object);

            this._notifier.NotifyMessageUpdate(new[] { userId }, message);

            mockObserver.Verify(mockObserver => mockObserver.OnMessageUpdateReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyReadReceipt_RegisteredUsers_ReceiveReadReceipt()
        {
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var readReceipt = new ReadReceiptDTO(1, 100, userId, DateTime.UtcNow);

            this._notifier.Register(userId, mockObserver.Object);

            this._notifier.NotifyReadReceipt(new[] { userId }, readReceipt);

            mockObserver.Verify(mockObserver => mockObserver.OnReadReceiptReceived(readReceipt), Times.Once);
        }

        [Fact]
        public void NotifyNewConversation_RegisteredParticipants_ReceiveConversationNotification()
        {
            var mockObserver1 = new Mock<IConversationService>();
            var mockObserver2 = new Mock<IConversationService>();

            this._notifier.Register(1, mockObserver1.Object);
            this._notifier.Register(2, mockObserver2.Object);

            var conversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
        {
            new ConversationParticipant { UserId = 1 },
            new ConversationParticipant { UserId = 2 }
        },
            };

            this._notifier.NotifyNewConversation(conversation);

            mockObserver1.Verify(mockObserver => mockObserver.OnConversationReceived(conversation), Times.Once);
            mockObserver2.Verify(mockObserver => mockObserver.OnConversationReceived(conversation), Times.Once);
        }

        [Fact]
        public void SnapshotSubscribers_UserNotRegistered_GracefullyIgnores()
        {
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object;

            this._notifier.Register(1, mockObserver.Object);

            this._notifier.NotifyMessage(new[] { 2 }, message);

            mockObserver.Verify(mockObserver => mockObserver.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }
    }
}
