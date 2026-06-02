//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using BoardGames.Desktop.Services;
//using BoardGames.Desktop.ViewModels;
//using BoardGames.Shared.DTO;
//using BoardGames.Shared.ProxyServices;
//using Moq;
//using NUnit.Framework;

//namespace BoardGames.Tests.ViewModels
//{
//    [TestFixture]
//    public sealed class DashboardViewModelTests
//    {
//        private Mock<IConversationService> mockConversationService = null!;
//        private Mock<INotificationService> mockNotificationService = null!;
//        private Mock<IPaymentService> mockPaymentService = null!;
//        private Mock<ISessionContext> mockSessionContext = null!;

//        [SetUp]
//        public void SetUp()
//        {
//            this.mockConversationService = new Mock<IConversationService>();
//            this.mockNotificationService = new Mock<INotificationService>();
//            this.mockPaymentService = new Mock<IPaymentService>();
//            this.mockSessionContext = new Mock<ISessionContext>();

//            this.mockSessionContext.Setup(s => s.IsLoggedIn).Returns(true);
//            this.mockSessionContext.Setup(s => s.AccountId).Returns(Guid.NewGuid());
//        }

//        [Test]
//        public async Task LoadDashboardDataAsync_ShouldUpdateCounts_WhenServicesReturnData()
//        {
//            var conversations = new List<ConversationDTO>
//            {
//                new ConversationDTO(1, new List<int> { 1, 2 }, new List<MessageDataTransferObject>(), new Dictionary<int, DateTime> { { 1, DateTime.MinValue } })
//            };
//            conversations[0].UnreadCount[1] = 5;

//            var notifications = new List<NotificationDTO>
//            {
//                new NotificationDTO { Id = 1 },
//                new NotificationDTO { Id = 2 }
//            };

//            this.mockConversationService
//                .Setup(s => s.GetConversationsForUserAsync(It.IsAny<Guid>()))
//                .ReturnsAsync(ServiceResult<List<ConversationDTO>>.Ok(conversations.ToList()));

//            this.mockNotificationService
//                .Setup(s => s.GetNotificationsForUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(ServiceResult<IReadOnlyList<NotificationDTO>>.Ok((IReadOnlyList<NotificationDTO>)notifications));

//            var viewModel = new DashboardViewModel(
//                mockConversationService.Object,
//                mockNotificationService.Object,
//                mockPaymentService.Object,
//                mockSessionContext.Object);

//            await Task.Delay(100);

//            Assert.That(viewModel.NewMessagesCount, Is.EqualTo(1));
//            Assert.That(viewModel.PendingNotificationsCount, Is.EqualTo(2));
//        }

//        [Test]
//        public async Task LoadDashboardDataAsync_ShouldKeepZeroCounts_WhenUserIsNotLoggedIn()
//        {
//            this.mockSessionContext.Setup(s => s.IsLoggedIn).Returns(false);

//            var viewModel = new DashboardViewModel(
//                mockConversationService.Object,
//                mockNotificationService.Object,
//                mockPaymentService.Object,
//                mockSessionContext.Object);

//            await Task.Delay(100);

//            Assert.That(viewModel.NewMessagesCount, Is.EqualTo(0));
//            Assert.That(viewModel.PendingNotificationsCount, Is.EqualTo(0));
//        }
//    }
//}