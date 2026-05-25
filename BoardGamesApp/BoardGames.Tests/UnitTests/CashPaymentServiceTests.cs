// <copyright file="CashPaymentServiceTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

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
            this._mockPaymentRepository = new Mock<IPaymentRepository>();
            this._mockCashPaymentMapper = new Mock<ICashPaymentMapper>();
            this._mockReceiptService = new Mock<IReceiptService>();

            this._cashPaymentService = new CashPaymentService(
                this._mockPaymentRepository.Object,
                this._mockCashPaymentMapper.Object,
                this._mockReceiptService.Object);
        }

        #region AddCashPaymentAsync

        [Fact]
        public async Task AddCashPaymentAsync_ValidData_ReturnsPaymentIdentifier()
        {
            var dto = new CashPaymentDTO(0, 100, 2, 3, 50.0m);
            var paymentEntity = new Payment();
            int expectedIdentifier = 10;

            this._mockCashPaymentMapper.Setup(cashPaymentMapper => cashPaymentMapper.TurnDTOIntoEntity(dto))
                                  .Returns(paymentEntity);

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.AddPaymentAsync(paymentEntity))
                                  .ReturnsAsync(expectedIdentifier);
            this._mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(100))
                               .Returns("receipts\\receipt_100.pdf");

            var result = await this._cashPaymentService.AddCashPaymentAsync(dto);

            Assert.Equal(expectedIdentifier, result);
            Assert.Equal("CASH", paymentEntity.PaymentMethod);
            Assert.Equal(PaymentConstrants.StateCompleted, paymentEntity.PaymentState);
            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.AddPaymentAsync(paymentEntity), Times.Once);
            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(paymentEntity), Times.Once);
        }

        #endregion

        #region GetCashPaymentAsync

        [Fact]
        public async Task GetCashPaymentAsync_ValidIdentifier_ReturnsMappedDTO()
        {
            int paymentId = 1;
            var paymentEntity = new Payment();
            var expectedDto = new CashPaymentDTO(0, 100, 2, 3, 50.0m);

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentEntity);

            this._mockCashPaymentMapper.Setup(cashPaymentMapper => cashPaymentMapper.TurnEntityIntoDTO(paymentEntity))
                                  .Returns(expectedDto);

            var result = await this._cashPaymentService.GetCashPaymentAsync(paymentId);

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
                DateConfirmedBuyer = null,
            };
            string expectedReceiptPath = "/receipts/100.pdf";

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            this._mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(expectedReceiptPath);

            await this._cashPaymentService.ConfirmDeliveryAsync(paymentId);

            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Equal(expectedReceiptPath, payment.ReceiptFilePath);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            this._mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId), Times.Once);
        }

        [Fact]
        public async Task ConfirmDeliveryAsync_SellerNotConfirmed_DoesNotGenerateReceipt()
        {
            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedSeller = null,
                DateConfirmedBuyer = null,
            };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            await this._cashPaymentService.ConfirmDeliveryAsync(paymentId);

            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Null(payment.ReceiptFilePath);
            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            this._mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
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
                DateConfirmedSeller = null,
            };
            string expectedReceiptPath = "/receipts/100.pdf";

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            this._mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(expectedReceiptPath);

            await this._cashPaymentService.ConfirmPaymentAsync(paymentId);

            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Equal(expectedReceiptPath, payment.ReceiptFilePath);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            this._mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(payment.RequestId), Times.Once);
        }

        [Fact]
        public async Task ConfirmPaymentAsync_BuyerNotConfirmed_DoesNotGenerateReceipt()
        {
            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null,
            };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            await this._cashPaymentService.ConfirmPaymentAsync(paymentId);

            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Null(payment.ReceiptFilePath);
            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(payment), Times.Once);
            this._mockReceiptService.Verify(receiptService => receiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
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
                PaymentState = PaymentConstrants.StateCompleted,
            };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            var result = await this._cashPaymentService.IsAllConfirmedAsync(paymentId);

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
                DateConfirmedBuyer = null,
            };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            var result = await this._cashPaymentService.IsAllConfirmedAsync(paymentId);

            Assert.False(result);
        }

        #endregion

        #region IsDeliveryConfirmedAsync

        [Fact]
        public async Task IsDeliveryConfirmedAsync_BuyerConfirmed_ReturnsTrue()
        {
            int paymentId = 1;
            var payment = new Payment { DateConfirmedBuyer = DateTime.Now };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            var result = await this._cashPaymentService.IsDeliveryConfirmedAsync(paymentId);

            Assert.True(result);
        }

        [Fact]
        public async Task IsDeliveryConfirmedAsync_BuyerNotConfirmed_ReturnsFalse()
        {
            int paymentId = 1;
            var payment = new Payment { DateConfirmedBuyer = null };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            var result = await this._cashPaymentService.IsDeliveryConfirmedAsync(paymentId);

            Assert.False(result);
        }

        #endregion

        #region IsPaymentConfirmedAsync

        [Fact]
        public async Task IsPaymentConfirmedAsync_SellerConfirmed_ReturnsTrue()
        {
            int paymentId = 1;
            var payment = new Payment { DateConfirmedSeller = DateTime.Now };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            var result = await this._cashPaymentService.IsPaymentConfirmedAsync(paymentId);

            Assert.True(result);
        }

        [Fact]
        public async Task IsPaymentConfirmedAsync_SellerNotConfirmed_ReturnsFalse()
        {
            int paymentId = 1;
            var payment = new Payment { DateConfirmedSeller = null };

            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            var result = await this._cashPaymentService.IsPaymentConfirmedAsync(paymentId);

            Assert.False(result);
        }

        #endregion
    }
}
