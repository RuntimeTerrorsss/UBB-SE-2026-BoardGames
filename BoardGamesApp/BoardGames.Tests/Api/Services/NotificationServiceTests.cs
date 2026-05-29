// <copyright file="NotificationServiceTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using BoardGames.Api.Mappers;
using BoardGames.Api.Services;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.Api.Services
{
    [TestFixture]
    public sealed class NotificationServiceTests
    {
        private FakeNotificationRepository notificationRepository = null!;
        private NotificationService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationRepository = new FakeNotificationRepository();
            this.service = new NotificationService(this.notificationRepository, new NotificationMapper(new UserMapper()));
        }

        [Test]
        public void SendNotificationToUser_SavesNotificationInRepository()
        {
            var recipientId = Guid.NewGuid();
            var notification = new NotificationDTO
            {
                Recipient = new UserDTO { Id = recipientId, DisplayName = "Receiver" },
                Title = "Hello",
                Body = "World",
                Type = NotificationType.Informational,
                RelatedRequestId = 42,
            };

            this.service.SendNotificationToUser(recipientId, notification);

            Notification savedNotification = this.notificationRepository.LastAddedNotification!;
            Assert.That(this.notificationRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(savedNotification.Recipient!.Id, Is.EqualTo(recipientId));
            Assert.That(savedNotification.Title, Is.EqualTo("Hello"));
            Assert.That(savedNotification.Body, Is.EqualTo("World"));
            Assert.That(savedNotification.RelatedRequest!.Id, Is.EqualTo(42));
        }

        [Test]
        public void DeleteNotificationsLinkedToRequest_CallsRepository()
        {
            this.service.DeleteNotificationsLinkedToRequest(42);

            Assert.That(this.notificationRepository.DeleteLinkedCallCount, Is.EqualTo(1));
            Assert.That(this.notificationRepository.LastLinkedRequestId, Is.EqualTo(42));
        }

        [Test]
        public void GetNotificationsForUser_ReturnsNotificationsForUser()
        {
            var userAccountId = Guid.NewGuid();
            var storedNotification = new Notification
            {
                Id = 1,
                Recipient = new Account { Id = userAccountId },
                Title = "New request",
                Body = "Someone wants to rent your game.",
            };

            this.notificationRepository.NotificationsByUser = ImmutableList.Create(storedNotification);

            var result = this.service.GetNotificationsForUser(userAccountId);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("New request"));
        }

        [Test]
        public void SendNotificationToUser_WithNullNotification_ThrowsArgumentNullException()
        {
            var recipientId = Guid.NewGuid();

            Assert.Throws<ArgumentNullException>(() =>
                this.service.SendNotificationToUser(recipientId, null!));
        }

        [Test]
        public void SendNotificationToUser_WithDefaultTimestamp_SetsTimestampToCurrentTime()
        {
            var recipientId = Guid.NewGuid();
            var notificationWithNoTimestamp = new NotificationDTO
            {
                Recipient = new UserDTO { Id = recipientId },
                Title = "Test",
                Body = "Test body",
                Timestamp = default,
            };

            this.service.SendNotificationToUser(recipientId, notificationWithNoTimestamp);

            Notification savedNotification = this.notificationRepository.LastAddedNotification!;
            Assert.That(savedNotification.Timestamp, Is.Not.EqualTo(default(DateTime)));
        }
    }
}
