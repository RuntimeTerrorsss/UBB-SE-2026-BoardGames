using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Services;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;

namespace BoardGames.Desktop.Tests.ViewModels
{
    public class ConfirmBookingViewModelTests
    {
        private readonly Mock<IRequestService> _mockRequestService;
        private readonly Mock<ISessionContext> _mockSessionContext;
        private readonly ConfirmBookingViewModel _viewModel;

        public ConfirmBookingViewModelTests()
        {
            _mockRequestService = new Mock<IRequestService>();
            _mockSessionContext = new Mock<ISessionContext>();

            _viewModel = new ConfirmBookingViewModel(
                _mockRequestService.Object,
                _mockSessionContext.Object);
        }

        [Fact]
        public void Initialize_SetsPropertiesAndCalculatesPriceCorrectly()
        {
            var game = new GameSummaryDTO { Price = 15m }; 
            var startDate = new DateTime(2026, 6, 1);
            var endDate = new DateTime(2026, 6, 3); 

            _viewModel.Initialize(game, startDate, endDate);

            Assert.Equal(3, _viewModel.NumberOfDays);
            Assert.Equal(45m, _viewModel.TotalPrice); 
            Assert.Equal(game, _viewModel.GameDetails);
        }

        [Fact]
        public async Task ConfirmBookingAsync_UserNotLoggedIn_RaisesError()
        {
            _mockSessionContext.Setup(s => s.AccountId).Returns(Guid.Empty);

            string? receivedError = null;
            _viewModel.OnErrorOccurred += (msg) => receivedError = msg;

            await _viewModel.ConfirmBookingAsync();

            Assert.NotNull(receivedError);
            Assert.Contains("not logged in", receivedError);
            _mockRequestService.Verify(r => r.CreateRequestAsync(It.IsAny<CreateRequestDTO>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmBookingAsync_UserOwnsGame_RaisesError()
        {
            var userId = Guid.NewGuid();
            _mockSessionContext.Setup(s => s.AccountId).Returns(userId);

            var game = new GameSummaryDTO { OwnerAccountId = userId }; 
            _viewModel.Initialize(game, DateTime.Now, DateTime.Now.AddDays(1));

            string? receivedError = null;
            _viewModel.OnErrorOccurred += (msg) => receivedError = msg;

            await _viewModel.ConfirmBookingAsync();

            Assert.NotNull(receivedError);
            Assert.Contains("cannot rent a game you already own", receivedError);
            _mockRequestService.Verify(r => r.CreateRequestAsync(It.IsAny<CreateRequestDTO>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmBookingAsync_ApiReturnsError_RaisesError()
        {
            var renterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid(); 

            _mockSessionContext.Setup(s => s.AccountId).Returns(renterId);
            var game = new GameSummaryDTO { OwnerAccountId = ownerId, GameId = 99 };
            _viewModel.Initialize(game, DateTime.Now, DateTime.Now.AddDays(1));

            var failedResponse = new ServiceResult { IsSuccess = false, ErrorMessage = "Dates overlap with existing booking." };
            _mockRequestService.Setup(r => r.CreateRequestAsync(It.IsAny<CreateRequestDTO>())).ReturnsAsync(failedResponse);

            string? receivedError = null;
            _viewModel.OnErrorOccurred += (msg) => receivedError = msg;

            await _viewModel.ConfirmBookingAsync();

            Assert.NotNull(receivedError);
            Assert.Equal("Dates overlap with existing booking.", receivedError);
        }

        [Fact]
        public async Task ConfirmBookingAsync_ValidRequest_FiresSuccessEvent()
        {
            var renterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            _mockSessionContext.Setup(s => s.AccountId).Returns(renterId);
            var game = new GameSummaryDTO { OwnerAccountId = ownerId, GameId = 99 };
            _viewModel.Initialize(game, DateTime.Now, DateTime.Now.AddDays(1));

            var successResponse = new ServiceResult { IsSuccess = true };
            _mockRequestService.Setup(r => r.CreateRequestAsync(It.IsAny<CreateRequestDTO>())).ReturnsAsync(successResponse);

            bool successEventFired = false;
            _viewModel.OnConfirmBookingRequested += () => successEventFired = true;

            await _viewModel.ConfirmBookingAsync();

            Assert.True(successEventFired, "The success event should have fired.");
            _mockRequestService.Verify(r => r.CreateRequestAsync(It.IsAny<CreateRequestDTO>()), Times.Once);
        }
    }
}