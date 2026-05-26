using System;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
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
            notificationRepository = new FakeNotificationRepository();
            service = new NotificationService(notificationRepository, new NotificationMapper(new UserMapper()));
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

            service.SendNotificationToUser(recipientId, notification);

            Notification savedNotification = notificationRepository.LastAddedNotification!;
            Assert.That(notificationRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(savedNotification.Recipient!.Id, Is.EqualTo(recipientId));
            Assert.That(savedNotification.Title, Is.EqualTo("Hello"));
            Assert.That(savedNotification.Body, Is.EqualTo("World"));
            Assert.That(savedNotification.RelatedRequest!.Id, Is.EqualTo(42));
        }

        [Test]
        public void DeleteNotificationsLinkedToRequest_CallsRepository()
        {
            service.DeleteNotificationsLinkedToRequest(42);

            Assert.That(notificationRepository.DeleteLinkedCallCount, Is.EqualTo(1));
            Assert.That(notificationRepository.LastLinkedRequestId, Is.EqualTo(42));
        }
    }
}
