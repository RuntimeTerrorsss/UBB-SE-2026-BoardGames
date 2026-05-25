using BoardGames.Data.Repositories;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly TestablePaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockReceiptService = new Mock<IReceiptService>();

            _paymentService = new TestablePaymentService(
                _mockPaymentRepository.Object,
                _mockReceiptService.Object);
        }

        #region GenerateReceiptAsync

        [Fact]
        public async Task GenerateReceiptAsync_PaymentNotFound_DoesNothing()
        {

            int paymentId = 1;
            _mockPaymentRepository.Setup(mockPaymentRepo => mockPaymentRepo.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync((Payment)null);


            await _paymentService.GenerateReceiptAsync(paymentId);


            _mockReceiptService.Verify(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
            _mockPaymentRepository.Verify(mockPaymentRepo => mockPaymentRepo.UpdatePaymentAsync(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public async Task GenerateReceiptAsync_PaymentFound_UpdatesPaymentWithReceiptPath()
        {

            int paymentId = 1;
            var payment = new Payment { RequestId = 100, ReceiptFilePath = null };
            string generatedPath = "/receipts/100.pdf";

            _mockPaymentRepository.Setup(mockPaymentRepo => mockPaymentRepo.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(generatedPath);


            await _paymentService.GenerateReceiptAsync(paymentId);


            Assert.Equal(generatedPath, payment.ReceiptFilePath);
            _mockPaymentRepository.Verify(mockPaymentRepo => mockPaymentRepo.UpdatePaymentAsync(payment), Times.Once);
        }

        #endregion

        #region GetReceiptAsync

        [Fact]
        public async Task GetReceiptAsync_PaymentNotFound_ReturnsEmptyString()
        {

            int paymentId = 1;
            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync((Payment)null);


            var result = await _paymentService.GetReceiptAsync(paymentId);


            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetReceiptAsync_ReceiptPathAlreadyExists_ReturnsDocumentDirectly()
        {

            int paymentId = 1;
            var payment = new Payment { RequestId = 100, ReceiptFilePath = "/receipts/100.pdf" };
            string expectedDocument = "Base64PDFContentOrFullPath";

            _mockPaymentRepository.Setup(mockPaymentRepo => mockPaymentRepo.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(mockReceiptService => mockReceiptService.GetReceiptDocument(payment))
                               .ReturnsAsync(expectedDocument);


            var result = await _paymentService.GetReceiptAsync(paymentId);


            Assert.Equal(expectedDocument, result);


            _mockReceiptService.Verify(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
            _mockPaymentRepository.Verify(mockPaymentRepo => mockPaymentRepo.UpdatePaymentAsync(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public async Task GetReceiptAsync_ReceiptPathEmpty_GeneratesPathAndReturnsDocument()
        {

            int paymentId = 1;
            var paymentWithoutPath = new Payment { RequestId = 100, ReceiptFilePath = null };
            var paymentWithPath = new Payment { RequestId = 100, ReceiptFilePath = "/receipts/100.pdf" };

            string generatedPath = "/receipts/100.pdf";
            string expectedDocument = "Base64PDFContentOrFullPath";





            _mockPaymentRepository.SetupSequence(mockPaymentRepo => mockPaymentRepo.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentWithoutPath)
                                  .ReturnsAsync(paymentWithoutPath)
                                  .ReturnsAsync(paymentWithPath);

            _mockReceiptService.Setup(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(paymentWithoutPath.RequestId))
                               .Returns(generatedPath);

            _mockReceiptService.Setup(mockReceiptService => mockReceiptService.GetReceiptDocument(paymentWithPath))
                               .ReturnsAsync(expectedDocument);


            var result = await _paymentService.GetReceiptAsync(paymentId);


            Assert.Equal(expectedDocument, result);
            _mockPaymentRepository.Verify(mockPaymentRepo => mockPaymentRepo.UpdatePaymentAsync(paymentWithoutPath), Times.Once);
        }

        [Fact]
        public async Task GetReceiptAsync_PaymentDisappearsAfterGeneration_ReturnsEmptyString()
        {

            int paymentId = 1;
            var paymentWithoutPath = new Payment { RequestId = 100, ReceiptFilePath = null };
            string generatedPath = "/receipts/100.pdf";


            _mockPaymentRepository.SetupSequence(mockPaymentRepo => mockPaymentRepo.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentWithoutPath)
                                  .ReturnsAsync(paymentWithoutPath)
                                  .ReturnsAsync((Payment)null);

            _mockReceiptService.Setup(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(paymentWithoutPath.RequestId))
                               .Returns(generatedPath);


            var result = await _paymentService.GetReceiptAsync(paymentId);


            Assert.Equal(string.Empty, result);


            _mockPaymentRepository.Verify(mockPaymentRepo => mockPaymentRepo.UpdatePaymentAsync(paymentWithoutPath), Times.Once);
            _mockReceiptService.Verify(mockReceiptService => mockReceiptService.GetReceiptDocument(It.IsAny<Payment>()), Times.Never);
        }

        #endregion




        private class TestablePaymentService : PaymentService
        {
            public TestablePaymentService(IPaymentRepository paymentRepository, IReceiptService receiptService)
                : base(paymentRepository, receiptService)
            {
            }
        }
    }
}