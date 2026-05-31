//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Threading.Tasks;
//using BoardGames.Data.Models;
//using BoardGames.Desktop.Services;
//using BoardGames.Desktop.ViewModels;
//using BoardGames.Shared.DTO;
//using BoardGames.Shared.ProxyServices;
//using Moq;
//using NUnit.Framework;

//namespace BoardGames.Tests.ViewModels
//{
//    [TestFixture]
//    public sealed class ChatViewModelTests
//    {
//        private Mock<IConversationService> mockConversationService = null!;
//        private Mock<ISessionContext> mockSessionContext = null!;
//        private ChatViewModel systemUnderTest = null!;

//        [SetUp]
//        public void SetUp()
//        {
//            this.mockConversationService = new Mock<IConversationService>();
//            this.mockSessionContext = new Mock<ISessionContext>();

//            this.mockSessionContext.Setup(s => s.AccountId).Returns(Guid.NewGuid());

//            this.systemUnderTest = new ChatViewModel(this.mockConversationService.Object, this.mockSessionContext.Object);
//        }

//        [Test]
//        public async Task LoadConversationAsync_ShouldPopulateMessages_WhenServiceReturnsData()
//        {
//            var messages = new List<MessageDataTransferObject>
//            {
//                new MessageDataTransferObject(1, 1, 1, 2, DateTime.UtcNow, "Salut", MessageType.MessageText, "", false, false, false, false, -1, -1)
//            };
//            var conversation = new ConversationDTO(1, new List<int> { 123, 456 }, messages, new Dictionary<int, DateTime>());

//            this.mockConversationService
//                .Setup(s => s.GetConversationByIdAsync(It.IsAny<int>()))
//                .ReturnsAsync(ServiceResult<ConversationDTO>.Ok(conversation));

//            await this.systemUnderTest.LoadConversationAsync(1);

//            Assert.That(this.systemUnderTest.Messages.Count, Is.EqualTo(1));
//            Assert.That(this.systemUnderTest.IsLoading, Is.False);
//        }

//        [Test]
//        public async Task SendTextMessageAsync_ShouldAddMessageToCollection_WhenSendIsSuccessful()
//        {
//            var conversation = new ConversationDTO(1, new List<int> { 123, 456 }, new List<MessageDataTransferObject>(), new Dictionary<int, DateTime>());

//            this.mockConversationService.Setup(s => s.GetConversationByIdAsync(It.IsAny<int>()))
//                .ReturnsAsync(ServiceResult<ConversationDTO>.Ok(conversation));
//            await this.systemUnderTest.LoadConversationAsync(1);

//            var sentMessage = new MessageDataTransferObject(2, 1, 123, 456, DateTime.UtcNow, "Test", MessageType.MessageText, "", false, false, false, false, -1, -1);

//            this.mockConversationService.Setup(s => s.SendMessageAsync(It.IsAny<MessageDataTransferObject>()))
//                .ReturnsAsync(ServiceResult<MessageDataTransferObject>.Ok(sentMessage));

//            await this.systemUnderTest.SendTextMessageAsync("Test");

//            Assert.That(this.systemUnderTest.Messages.Any(m => m.Content == "Test"), Is.True);
//        }

//        [Test]
//        public async Task SendReadReceiptAsync_ShouldCallService_WhenConversationIsLoaded()
//        {
//            var conversation = new ConversationDTO(1, new List<int> { 123, 456 }, new List<MessageDataTransferObject>(), new Dictionary<int, DateTime>());

//            this.mockConversationService.Setup(s => s.GetConversationByIdAsync(1))
//                .ReturnsAsync(ServiceResult<ConversationDTO>.Ok(conversation));
//            await this.systemUnderTest.LoadConversationAsync(1);

//            await this.systemUnderTest.SendReadReceiptAsync();

//            this.mockConversationService.Verify(s => s.SendReadReceiptAsync(It.IsAny<ReadReceiptDTO>()), Times.Once);
//        }
//    }
//}