using System;
using System.Threading.Tasks;
using BoardGames.Data.Constants;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Api.Mappers;
using BoardGames.Api.Services;
using Moq;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class CashPaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<ICashPaymentMapper> _mockCashPaymentMapper;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly CashPaymentService _cashPaymentService;

        public CashPaymentServiceTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockCashPaymentMapper = new Mock<ICashPaymentMapper>();
            _mockReceiptService = new Mock<IReceiptService>();

            _cashPaymentService = new CashPaymentService(
                _mockPaymentRepository.Object,
                _mockCashPaymentMapper.Object,
                _mockReceiptService.Object);
        }

        #region AddCashPaymentAsync

        [Fact]
        public async Task AddCashPaymentAsync_ValidData_ReturnsPaymentIdentifier()
        {

            var dto = new CashPaymentDataTransferObject(0, 100, 2, 3, 50.0m);
            var paymentEntity = new Payment();
            int expectedIdentifier = 10;

            _mockCashPaymentMapper.Setup(cashPaymentMapper => cashPaymentMapper.TurnDataTransferObjectIntoEntity(dto))
                                  .Returns(paymentEntity);

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.AddPaymentAsync(paymentEntity))
                                  .ReturnsAsync(expectedIdentifier);
            _mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(100))
                               .Returns("receipts\\receipt_100.pdf");

            var result = await _cashPaymentService.AddCashPaymentAsync(dto);


            Assert.Equal(expectedIdentifier, result);
            Assert.Equal("CASH", paymentEntity.PaymentMethod);
            Assert.Equal(PaymentConstrants.StateCompleted, paymentEntity.PaymentState);
            _mockPaymentRepository.Verify(paymentRepository => paymentRepository.AddPaymentAsync(paymentEntity), Times.Once);
            _mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(paymentEntity), Times.Once);
        }

        #endregion

        #region GetCashPaymentAsync

        [Fact]
        public async Task GetCashPaymentAsync_ValidIdentifier_ReturnsMappedDTO()
        {

            int paymentId = 1;
            var paymentEntity = new Payment();
            var expectedDto = new CashPaymentDataTransferObject(0, 100, 2, 3, 50.0m);

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentEntity);

            _mockCashPaymentMapper.Setup(cashPaymentMapper => cashPaymentMapper.TurnEntityIntoDataTransferObject(paymentEntity))
                                  .Returns(expectedDto);


            var result = await _cashPaymentService.GetCashPaymentAsync(paymentId);


            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);
        }

        #endregion

        #region ConfirmDeliveryAsync

        [Fact]
        public async Task ConfirmDeliveryAsync_AllConfirmed_GeneratesReceiptAndUpdates()
        {

            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = null
            };
            string expectedReceiptPath = "/receipts/100.pdf";

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(expectedReceiptPath);


            await _cashPaymentService.ConfirmDeliveryAsync(paymentId);


            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Equal(expectedReceiptPath, payment.ReceiptFilePath);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
            _mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId), Times.Once);
        }

        [Fact]
        public async Task ConfirmDeliveryAsync_SellerNotConfirmed_DoesNotGenerateReceipt()
        {

            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedSeller = null,
                DateConfirmedBuyer = null
            };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            await _cashPaymentService.ConfirmDeliveryAsync(paymentId);


            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Null(payment.ReceiptFilePath);
            _mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region ConfirmPaymentAsync

        [Fact]
        public async Task ConfirmPaymentAsync_AllConfirmed_GeneratesReceiptAndUpdates()
        {

            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null
            };
            string expectedReceiptPath = "/receipts/100.pdf";

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(expectedReceiptPath);


            await _cashPaymentService.ConfirmPaymentAsync(paymentId);


            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Equal(expectedReceiptPath, payment.ReceiptFilePath);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
            _mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId), Times.Once);
        }

        [Fact]
        public async Task ConfirmPaymentAsync_BuyerNotConfirmed_DoesNotGenerateReceipt()
        {

            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null
            };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            await _cashPaymentService.ConfirmPaymentAsync(paymentId);


            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Null(payment.ReceiptFilePath);
            _mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region IsAllConfirmedAsync

        [Fact]
        public async Task IsAllConfirmedAsync_BothConfirmed_ReturnsTrueAndSetsState()
        {

            int paymentId = 1;
            var payment = new Payment
            {
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                PaymentState = PaymentConstrants.StateCompleted
            };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            var result = await _cashPaymentService.IsAllConfirmedAsync(paymentId);


            Assert.True(result);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
        }

        [Fact]
        public async Task IsAllConfirmedAsync_MissingConfirmation_ReturnsFalse()
        {

            int paymentId = 1;
            var payment = new Payment
            {
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = null
            };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            var result = await _cashPaymentService.IsAllConfirmedAsync(paymentId);


            Assert.False(result);
        }

        #endregion

        #region IsDeliveryConfirmedAsync

        [Fact]
        public async Task IsDeliveryConfirmedAsync_BuyerConfirmed_ReturnsTrue()
        {

            int paymentId = 1;
            var payment = new Payment { DateConfirmedBuyer = DateTime.Now };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            var result = await _cashPaymentService.IsDeliveryConfirmedAsync(paymentId);


            Assert.True(result);
        }

        [Fact]
        public async Task IsDeliveryConfirmedAsync_BuyerNotConfirmed_ReturnsFalse()
        {

            int paymentId = 1;
            var payment = new Payment { DateConfirmedBuyer = null };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            var result = await _cashPaymentService.IsDeliveryConfirmedAsync(paymentId);


            Assert.False(result);
        }

        #endregion

        #region IsPaymentConfirmedAsync

        [Fact]
        public async Task IsPaymentConfirmedAsync_SellerConfirmed_ReturnsTrue()
        {

            int paymentId = 1;
            var payment = new Payment { DateConfirmedSeller = DateTime.Now };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            var result = await _cashPaymentService.IsPaymentConfirmedAsync(paymentId);


            Assert.True(result);
        }

        [Fact]
        public async Task IsPaymentConfirmedAsync_SellerNotConfirmed_ReturnsFalse()
        {

            int paymentId = 1;
            var payment = new Payment { DateConfirmedSeller = null };

            _mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);


            var result = await _cashPaymentService.IsPaymentConfirmedAsync(paymentId);


            Assert.False(result);
        }

        #endregion
    }
}