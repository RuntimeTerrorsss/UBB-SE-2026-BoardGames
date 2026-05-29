//using BoardGames.Data.Repositories;
//using Xunit;
//using BoardGames.Data.Enums;
//using BoardGames.Api.Legacy.Services;
//using Moq;
//// <copyright file="ServicePaymentTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace BoardGames.Tests.UnitTests
//{
//    public class ServicePaymentTests : IDisposable
//    {
//        private readonly Mock<IRepositoryPayment> _mockPaymentRepository;
//        private readonly Mock<IReceiptService> _mockReceiptService;
//        private readonly Mock<IRentalService> _mockRentalService;
//        private readonly Mock<IConversationService> _mockConversationService;
//        private readonly ServicePayment _service;

//        public ServicePaymentTests()
//        {
//            this._mockPaymentRepository = new Mock<IRepositoryPayment>();
//            this._mockReceiptService = new Mock<IReceiptService>();
//            this._mockRentalService = new Mock<IRentalService>();
//            this._mockConversationService = new Mock<IConversationService>();
//            this._mockRentalService.Setup(mockRentalService => mockRentalService.GetRentalsForUser(It.IsAny<int>())).ReturnsAsync(new List<RentalDTO>());
//            this._mockConversationService.Setup(mockConversationService => mockConversationService.FetchConversations()).ReturnsAsync(new List<ConversationDTO>());
//            this._service = new ServicePayment(
//                this._mockPaymentRepository.Object,
//                this._mockReceiptService.Object,
//                this._mockRentalService.Object,
//                this._mockConversationService.Object);

//            SessionContext.GetInstance().UserId = 1;
//        }

//        public void Dispose()
//        {
//            SessionContext.GetInstance().UserId = 0;
//        }

//        #region GetAllPaymentsForUI & CurrentUser Filtering

//        [Fact]
//        public async Task GetAllPaymentsForUI_SessionUserIdZero_ReturnsEmptyList()
//        {
//            SessionContext.GetInstance().UserId = 0;
//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(this.GetDummyHistoryPayments());

//            var result = await this._service.GetAllPaymentsForUI();

//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task GetAllPaymentsForUI_ValidSessionUser_FiltersByClientOrOwnerAndMapsCorrectly()
//        {
//            SessionContext.GetInstance().UserId = 1;
//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(this.GetDummyHistoryPayments());

//            var result = await this._service.GetAllPaymentsForUI();

//            Assert.Equal(2, result.Count);

//            var paymentWithNulls = result.First(payment => payment.PaymentId == 2);
//            Assert.Equal(PaymentHistoryConstants.NullGameNameDefaultValue, paymentWithNulls.ProductName);
//            Assert.Equal(PaymentHistoryConstants.NullOwnerNameDefaultValue, paymentWithNulls.ReceiverName);
//            Assert.Equal(PaymentHistoryConstants.NullDateOfTransactionDefaultValue, paymentWithNulls.DateText);
//        }

//        #endregion

//        #region GetFilteredPayments - Date Filters

//        [Theory]
//        [InlineData(FilterType.Last3Months, 1)]
//        [InlineData(FilterType.Last6Months, 2)]
//        [InlineData(FilterType.Last9Months, 3)]
//        [InlineData(FilterType.AllTime, 4)]
//        public async Task GetFilteredPayments_DateFilters_AppliesDateThresholdsCorrectly(FilterType filter, int expectedCount)
//        {
//            SessionContext.GetInstance().UserId = 1;

//            var payments = new List<HistoryPayment>
//            {
//                new HistoryPayment { TransactionIdentifier = 1, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-1) },
//                new HistoryPayment { TransactionIdentifier = 2, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-4) },
//                new HistoryPayment { TransactionIdentifier = 3, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-8) },
//                new HistoryPayment { TransactionIdentifier = 4, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-12) },
//            };

//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(payments);

//            var result = await this._service.GetFilteredPayments(filter);

//            Assert.Equal(expectedCount, result.TotalCount);
//            Assert.Equal(expectedCount, result.Items.Count());
//        }

//        #endregion

//        #region GetFilteredPayments - Search and Payment Method

//        [Fact]
//        public async Task GetFilteredPayments_PaymentMethodApplied_FiltersCorrectly()
//        {
//            SessionContext.GetInstance().UserId = 1;
//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(this.GetDummyHistoryPayments());

//            var result = await this._service.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CASH);

//            Assert.Single(result.Items);
//            Assert.Equal("cash", result.Items.First().PaymentMethod);
//        }

//        [Fact]
//        public async Task GetFilteredPayments_SearchQueryApplied_FiltersByGameNameCaseInsensitive()
//        {
//            SessionContext.GetInstance().UserId = 1;
//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(this.GetDummyHistoryPayments());

//            var result = await this._service.GetFilteredPayments(FilterType.AllTime, PaymentMethod.ALL, "cata");

//            Assert.Single(result.Items);
//            Assert.Equal("Catan", result.Items.First().ProductName);
//        }

//        #endregion

//        #region GetFilteredPayments - Sorting

//        [Theory]
//        [InlineData(FilterType.AlphabeticalAsc, "Catan")]
//        [InlineData(FilterType.AlphabeticalDesc, PaymentHistoryConstants.NullGameNameDefaultValue)]
//        [InlineData(FilterType.Newest, "Catan")]
//        [InlineData(FilterType.Oldest, PaymentHistoryConstants.NullGameNameDefaultValue)]
//        public async Task GetFilteredPayments_Sorting_OrdersItemsCorrectly(FilterType filter, string expectedFirstProductName)
//        {
//            SessionContext.GetInstance().UserId = 1;
//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(this.GetDummyHistoryPayments());

//            var result = await this._service.GetFilteredPayments(filter);

//            Assert.Equal(expectedFirstProductName, result.Items.First().ProductName);
//        }

//        #endregion

//        #region GetFilteredPayments - Pagination

//        [Fact]
//        public async Task GetFilteredPayments_Pagination_SkipsAndTakesCorrectly()
//        {
//            SessionContext.GetInstance().UserId = 1;
//            var payments = Enumerable.Range(1, 15).Select(i => new HistoryPayment { TransactionIdentifier = i, ClientId = 1 }).ToList();
//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetAllPayments()).ReturnsAsync(payments);

//            var result = await this._service.GetFilteredPayments(FilterType.AllTime, pageNumber: 2, pageSize: 5);

//            Assert.Equal(15, result.TotalCount);
//            Assert.Equal(5, result.Items.Count());
//            Assert.Equal(6, result.Items.First().PaymentId);
//            Assert.Equal(10, result.Items.Last().PaymentId);
//        }

//        #endregion

//        #region CalculateTotalAmount

//        [Fact]
//        public void CalculateTotalAmount_NullList_ReturnsDefaultValue()
//        {
//            var result = this._service.CalculateTotalAmount(null);

//            Assert.Equal(PaymentHistoryConstants.NullAmountDefaultValue, result);
//        }

//        [Fact]
//        public void CalculateTotalAmount_ValidList_ReturnsSumOfAmounts()
//        {
//            var items = new List<PaymentDTO>
//            {
//                new PaymentDTO { Amount = 10.5m },
//                new PaymentDTO { Amount = 20.0m },
//            };

//            var result = this._service.CalculateTotalAmount(items);

//            Assert.Equal(30.5m, result);
//        }

//        #endregion

//        #region GetReceiptDocumentPath

//        [Fact]
//        public async Task GetReceiptDocumentPath_NullPath_GeneratesPathAndFetchesDocument()
//        {
//            var payment = new HistoryPayment { RequestId = 5, ReceiptFilePath = null };
//            string generatedPath = "receipts\\generated_123.pdf";
//            string fullPath = "C:\\Documents\\receipts\\generated_123.pdf";

//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetPaymentById(1)).ReturnsAsync(payment);
//            this._mockReceiptService.Setup(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(5)).Returns(generatedPath);
//            this._mockReceiptService.Setup(mockReceiptService => mockReceiptService.GetReceiptDocument(payment)).ReturnsAsync(fullPath);

//            var result = await this._service.GetReceiptDocumentPath(1);

//            Assert.Equal(generatedPath, payment.ReceiptFilePath);
//            Assert.Equal(fullPath, result);
//            this._mockReceiptService.Verify(mockReceiptService => mockReceiptService.GetReceiptDocument(payment), Times.Once);
//        }

//        [Fact]
//        public async Task GetReceiptDocumentPath_PathWithoutSlash_PrependsReceiptsFolder()
//        {
//            var payment = new HistoryPayment { RequestId = 5, ReceiptFilePath = "no_slash_file.pdf" };

//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetPaymentById(1)).ReturnsAsync(payment);
//            this._mockReceiptService.Setup(mockReceiptService => mockReceiptService.GetReceiptDocument(payment)).ReturnsAsync("C:\\full\\path.pdf");

//            await this._service.GetReceiptDocumentPath(1);

//            Assert.Equal("receipts\\no_slash_file.pdf", payment.ReceiptFilePath);
//            this._mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
//        }

//        [Fact]
//        public async Task GetReceiptDocumentPath_ValidPath_FetchesDocumentDirectly()
//        {
//            var payment = new HistoryPayment { RequestId = 5, ReceiptFilePath = "receipts\\valid_file.pdf" };

//            this._mockPaymentRepository.Setup(mockPaymentRepository => mockPaymentRepository.GetPaymentById(1)).ReturnsAsync(payment);
//            this._mockReceiptService.Setup(mockReceiptService => mockReceiptService.GetReceiptDocument(payment)).ReturnsAsync("C:\\full\\path.pdf");

//            await this._service.GetReceiptDocumentPath(1);

//            Assert.Equal("receipts\\valid_file.pdf", payment.ReceiptFilePath);
//            this._mockReceiptService.Verify(mockReceiptService => mockReceiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
//        }

//        #endregion

//        #region Helper Data Providers

//        private List<HistoryPayment> GetDummyHistoryPayments()
//        {
//            return new List<HistoryPayment>
//            {
//                new HistoryPayment
//                {
//                    TransactionIdentifier = 1, ClientId = 1, OwnerId = 99,
//                    DateOfTransaction = DateTime.Now, GameName = "Catan", OwnerName = "Alice",
//                    PaidAmount = 15m, PaymentMethod = "card", ReceiptFilePath = "receipts\\1.pdf",
//                },

//                new HistoryPayment
//                {
//                    TransactionIdentifier = 2, ClientId = 99, OwnerId = 1,
//                    DateOfTransaction = null, GameName = null, OwnerName = null,
//                    PaidAmount = 25m, PaymentMethod = "cash", ReceiptFilePath = null,
//                },

//                new HistoryPayment
//                {
//                    TransactionIdentifier = 3, ClientId = 99, OwnerId = 100,
//                    DateOfTransaction = DateTime.Now, GameName = "Monopoly"
//                },
//            };
//        }

//        #endregion
//    }
//}
