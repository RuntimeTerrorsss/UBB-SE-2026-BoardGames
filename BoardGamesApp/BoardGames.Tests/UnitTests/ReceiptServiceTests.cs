using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;



namespace BoardGames.Tests.UnitTests
{
    public class ReceiptServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRentalService> _mockRentalService;
        private readonly Mock<InterfaceGamesRepository> _mockGameRepository;
        private readonly ReceiptService _receiptService;


        private readonly List<string> _filesToCleanup = new();

        public ReceiptServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRentalService = new Mock<IRentalService>();
            _mockGameRepository = new Mock<InterfaceGamesRepository>();

            _receiptService = new ReceiptService(
                _mockUserRepository.Object,
                _mockRentalService.Object,
                _mockGameRepository.Object);

            SetupDefaultMocks();
        }

        public void Dispose()
        {

            foreach (var filePath in _filesToCleanup)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void SetupDefaultMocks()
        {
            _mockRentalService.Setup(mockRentalService => mockRentalService.GetRentalById(It.IsAny<int>()))
                .ReturnsAsync(new Rental { GameId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(3) });

            _mockGameRepository.Setup(mockGameRepository => mockGameRepository.GetGameById(It.IsAny<int>()))
                .ReturnsAsync(new Game { Name = "Test Boardgame" });

            _mockUserRepository.Setup(mockUserRepository => mockUserRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync(new User { Username = "TestUser" });
        }

        #region GenerateReceiptRelativePath

        [Fact]
        public void GenerateReceiptRelativePath_ValidId_ReturnsExpectedFormat()
        {

            int requestId = 99;


            string result = _receiptService.GenerateReceiptRelativePath(requestId);


            Assert.NotNull(result);
            Assert.StartsWith("receipts\\receipt_99_", result);
            Assert.EndsWith(".pdf", result);
        }

        #endregion

        #region GetReceiptDocument - Edge Cases

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetReceiptDocument_ReceiptPathIsNullOrEmpty_ThrowsInvalidOperationException(string invalidPath)
        {

            var payment = new Payment { ReceiptFilePath = invalidPath };


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _receiptService.GetReceiptDocument(payment));

            Assert.Equal("Receipt path is missing.", exception.Message);
        }

        [Fact]
        public async Task GetReceiptDocument_ReceiptPathIsWhiteSpace_ThrowsFromPrepareDocumentPath()
        {



            var payment = new Payment { ReceiptFilePath = "   " };


            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _receiptService.GetReceiptDocument(payment));

            Assert.Equal("Receipt path is missing.", exception.Message);
        }

        #endregion

        #region GetReceiptDocument - File Exists Logic

        [Fact]
        public async Task GetReceiptDocument_FileAlreadyExists_ReturnsPathWithoutRecreating()
        {

            var payment = new Payment
            {
                RequestId = 1,
                PaymentMethod = "card",
                ReceiptFilePath = "receipts\\test_existing_receipt.pdf"
            };


            string createdPath = await _receiptService.GetReceiptDocument(payment);
            _filesToCleanup.Add(createdPath);


            string existingPath = await _receiptService.GetReceiptDocument(payment);


            Assert.Equal(createdPath, existingPath);
            Assert.True(File.Exists(existingPath));


            _mockRentalService.Verify(mockRentalService => mockRentalService.GetRentalById(It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region GetReceiptDocument - Creation and Content Generation Branches

        [Fact]
        public async Task GetReceiptDocument_FileDoesNotExist_CashPayment_CreatesPdfAndHitsCatchBlockForDate()
        {

            var payment = new Payment
            {
                RequestId = 2,
                ClientId = 10,
                OwnerId = 20,
                PaidAmount = 50m,
                PaymentMethod = "cash",
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,


                ReceiptFilePath = "receipts\\bad_format_name.pdf"
            };


            string generatedPath = await _receiptService.GetReceiptDocument(payment);
            _filesToCleanup.Add(generatedPath);


            Assert.True(File.Exists(generatedPath));

            _mockRentalService.Verify(mockRentalService => mockRentalService.GetRentalById(payment.RequestId), Times.Once);
            _mockGameRepository.Verify(mockGameRepository => mockGameRepository.GetGameById(It.IsAny<int>()), Times.Once);
            _mockUserRepository.Verify(mockUserRepository => mockUserRepository.GetById(payment.ClientId), Times.Once);
            _mockUserRepository.Verify(mockUserRepository => mockUserRepository.GetById(payment.OwnerId), Times.Once);
        }

        [Fact]
        public async Task GetReceiptDocument_FileDoesNotExist_CardPayment_CreatesPdfWithValidGeneratedPath()
        {

            int requestId = 3;

            string validRelativePath = _receiptService.GenerateReceiptRelativePath(requestId);

            var payment = new Payment
            {
                RequestId = requestId,
                ClientId = 10,
                OwnerId = 20,
                PaidAmount = 100m,
                PaymentMethod = "card",
                DateOfTransaction = DateTime.Now,
                ReceiptFilePath = validRelativePath
            };


            string generatedPath = await _receiptService.GetReceiptDocument(payment);
            _filesToCleanup.Add(generatedPath);


            Assert.True(File.Exists(generatedPath));

            _mockRentalService.Verify(mockRentalService => mockRentalService.GetRentalById(payment.RequestId), Times.Once);
        }

        #endregion
    }
}