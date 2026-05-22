using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class NotificationsViewModelTests
    {
        private readonly Guid currentUserId = Guid.NewGuid();
        private FakeClientNotificationService notificationService = null!;
        private FakeCurrentUserContext currentUserContext = null!;
        private FakeServerClient serverClient = null!;

        [SetUp]
        public void SetUp()
        {
            notificationService = new FakeClientNotificationService();
            currentUserContext = new FakeCurrentUserContext { CurrentUserId = currentUserId };
            serverClient = new FakeServerClient
            {
                ConnectionStatus = NotificationConnectionStatus.Connected,
            };
        }

        [Test]
        public async Task Constructor_LoadsNotificationsForCurrentUser()
        {
            notificationService.NotificationsForUser = ImmutableList.Create(
                    new NotificationDTO { Id = 1, Recipient = new UserDTO { Id = currentUserId }, Title = "a", Body = "b" },
                    new NotificationDTO { Id = 2, Recipient = new UserDTO { Id = currentUserId }, Title = "c", Body = "d" });

            using var viewModel = new NotificationsViewModel(
                notificationService,
                currentUserContext,
                serverClient);
            await viewModel.LoadCurrentUserNotificationsAsync();

            Assert.That(viewModel.PagedItems.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteNotificationByIdentifier_CallsServiceDelete()
        {
            notificationService.NotificationsForUser = ImmutableList<NotificationDTO>.Empty;

            using var viewModel = new NotificationsViewModel(
                notificationService,
                currentUserContext,
                serverClient);

            await viewModel.DeleteNotificationByIdentifierAsync(7);

            Assert.That(notificationService.DeleteNotificationCallCount, Is.EqualTo(1));
            Assert.That(notificationService.LastDeletedNotificationId, Is.EqualTo(7));
        }
    }
}
