//using BoardGames.Data.Repositories;
//using Xunit;
//using BoardGames.Api.Legacy.Services;
//using Moq;
//// <copyright file="CardPaymentServiceTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using System.Threading.Tasks;

//namespace BoardGames.Tests.UnitTests
//{
//    public class CardPaymentServiceTests
//    {
//        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
//        private readonly Mock<IUserRepository> _mockUserRepository;
//        private readonly Mock<IReceiptService> _mockReceiptService;
//        private readonly Mock<IRentalService> _mockRentalService;
//        private readonly CardPaymentService _cardPaymentService;

//        public CardPaymentServiceTests()
//        {
//            this._mockPaymentRepository = new Mock<IPaymentRepository>();
//            this._mockUserRepository = new Mock<IUserRepository>();
//            this._mockReceiptService = new Mock<IReceiptService>();
//            this._mockRentalService = new Mock<IRentalService>();

//            this._cardPaymentService = new CardPaymentService(
//                this._mockPaymentRepository.Object,
//                this._mockUserRepository.Object,
//                this._mockReceiptService.Object,
//                this._mockRentalService.Object);
//        }

//        #region AddCardPayment

//        [Fact]
//        public async Task AddCardPayment_InsufficientBalance_ThrowsException()
//        {
//            int requestId = 1, clientId = 2, ownerId = 3;
//            decimal amount = 50m;

//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(requestId)).ReturnsAsync(100m);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(clientId)).ReturnsAsync(50m);

//            var exception = await Assert.ThrowsAsync<Exception>(() =>
//                this._cardPaymentService.AddCardPayment(requestId, clientId, ownerId, amount));

//            Assert.Equal("Insufficient Funds", exception.Message);
//        }

//        [Fact]
//        public async Task AddCardPayment_ValidData_ProcessesPaymentAndReturnsDTO()
//        {
//            int requestId = 1, clientId = 2, ownerId = 3;
//            decimal amount = 50m;
//            decimal rentalPrice = 50m;
//            decimal clientBalance = 100m;
//            decimal ownerBalance = 200m;
//            int newTransactionId = 99;
//            string receiptPath = "/receipts/1.pdf";

//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(requestId)).ReturnsAsync(rentalPrice);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(clientId)).ReturnsAsync(clientBalance);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(ownerId)).ReturnsAsync(ownerBalance);

//            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.AddPaymentAsync(It.IsAny<Payment>())).ReturnsAsync(newTransactionId);
//            this._mockReceiptService.Setup(receiptService => receiptService.GenerateReceiptRelativePath(requestId)).Returns(receiptPath);

//            var result = await this._cardPaymentService.AddCardPayment(requestId, clientId, ownerId, amount);

//            Assert.NotNull(result);
//            Assert.Equal(newTransactionId, result.TransactionIdentifier);
//            Assert.Equal(requestId, result.RequestIdentifier);

//            this._mockUserRepository.Verify(userRepository => userRepository.UpdateBalance(clientId, clientBalance - rentalPrice), Times.Once);
//            this._mockUserRepository.Verify(userRepository => userRepository.UpdateBalance(ownerId, ownerBalance + rentalPrice), Times.Once);
//            this._mockPaymentRepository.Verify(paymentRepository => paymentRepository.UpdatePaymentAsync(It.Is<Payment>(pay => pay.ReceiptFilePath == receiptPath)), Times.Once);
//        }

//        #endregion

//        #region CheckBalanceSufficiency

//        [Theory]
//        [InlineData(50, 100, true)]
//        [InlineData(100, 100, true)]
//        [InlineData(150, 100, false)]
//        public async Task CheckBalanceSufficiency_VariousBalances_ReturnsExpectedResult(decimal price, decimal balance, bool expectedResult)
//        {
//            int requestId = 1, clientId = 2;
//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(requestId)).ReturnsAsync(price);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(clientId)).ReturnsAsync(balance);

//            var result = await this._cardPaymentService.CheckBalanceSufficiency(requestId, clientId);

//            Assert.Equal(expectedResult, result);
//        }

//        #endregion

//        #region GetCardPaymentAsync

//        [Fact]
//        public async Task GetCardPaymentAsync_PaymentNotFound_ReturnsNull()
//        {
//            int paymentId = 1;
//            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId)).ReturnsAsync((Payment)null);

//            var result = await this._cardPaymentService.GetCardPaymentAsync(paymentId);

//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task GetCardPaymentAsync_PaymentFound_ReturnsDTO()
//        {
//            int paymentId = 1;
//            var payment = new Payment
//            {
//                TransactionIdentifier = paymentId,
//                RequestId = 2,
//                ClientId = 3,
//                OwnerId = 4,
//                PaidAmount = 100m,
//                PaymentMethod = CardPaymentConstants.CardPaymentMethodName,
//                DateOfTransaction = DateTime.Now,
//            };

//            this._mockPaymentRepository.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifierAsync(paymentId)).ReturnsAsync(payment);

//            var result = await this._cardPaymentService.GetCardPaymentAsync(paymentId);

//            Assert.NotNull(result);
//            Assert.Equal(paymentId, result.TransactionIdentifier);
//            Assert.Equal(payment.PaidAmount, result.Amount);
//        }

//        #endregion

//        #region GetCurrentBalance

//        [Fact]
//        public async Task GetCurrentBalance_ValidClient_ReturnsBalance()
//        {
//            int clientId = 1;
//            decimal expectedBalance = 250.5m;
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(clientId)).ReturnsAsync(expectedBalance);

//            var result = await this._cardPaymentService.GetCurrentBalance(clientId);

//            Assert.Equal(expectedBalance, result);
//        }

//        #endregion

//        #region ProcessPayment

//        [Fact]
//        public async Task ProcessPayment_InsufficientFunds_ThrowsException()
//        {
//            int rentalId = 1, clientId = 2, ownerId = 3;
//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(rentalId)).ReturnsAsync(100m);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(clientId)).ReturnsAsync(50m);

//            var exception = await Assert.ThrowsAsync<Exception>(() =>
//                this._cardPaymentService.ProcessPayment(rentalId, clientId, ownerId));

//            Assert.Equal("Insufficient Funds", exception.Message);
//        }

//        [Fact]
//        public async Task ProcessPayment_SufficientFunds_UpdatesBalances()
//        {
//            int rentalId = 1, clientId = 2, ownerId = 3;
//            decimal rentalPrice = 100m;
//            decimal clientBalance = 150m;
//            decimal ownerBalance = 200m;

//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(rentalId)).ReturnsAsync(rentalPrice);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(clientId)).ReturnsAsync(clientBalance);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetUserBalance(ownerId)).ReturnsAsync(ownerBalance);

//            await this._cardPaymentService.ProcessPayment(rentalId, clientId, ownerId);

//            this._mockUserRepository.Verify(userRepository => userRepository.UpdateBalance(clientId, 50m), Times.Once);
//            this._mockUserRepository.Verify(userRepository => userRepository.UpdateBalance(ownerId, 300m), Times.Once);
//        }

//        #endregion

//        #region ConvertToDTO

//        [Fact]
//        public void ConvertToDTO_DateNull_UsesCurrentDate()
//        {
//            var payment = new Payment
//            {
//                TransactionIdentifier = 1,
//                DateOfTransaction = null,
//            };
//            var beforeExecution = DateTime.Now;

//            var result = this._cardPaymentService.ConvertToDTO(payment);

//            Assert.True(result.DateOfTransaction >= beforeExecution);
//            Assert.True(result.DateOfTransaction <= DateTime.Now);
//        }

//        [Fact]
//        public void ConvertToDTO_DateNotNull_UsesProvidedDate()
//        {
//            var specificDate = new DateTime(2025, 1, 1);
//            var payment = new Payment
//            {
//                TransactionIdentifier = 1,
//                DateOfTransaction = specificDate,
//            };

//            var result = this._cardPaymentService.ConvertToDTO(payment);

//            Assert.Equal(specificDate, result.DateOfTransaction);
//        }

//        #endregion

//        #region GetRequestDTO

//        [Fact]
//        public async Task GetRequestDTO_RentalIsNull_ThrowsInvalidOperationException()
//        {
//            int rentalId = 1;
//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalById(rentalId)).ReturnsAsync((Rental)null);

//            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
//                this._cardPaymentService.GetRequestDTO(rentalId));

//            Assert.Equal($"Rental with ID {rentalId} was not found.", exception.Message);
//        }

//        [Fact]
//        public async Task GetRequestDTO_UsersAreNull_UsesFallbackNames()
//        {
//            int rentalId = 1;
//            var rental = new Rental { RentalId = rentalId, GameId = 2, OwnerId = 3, ClientId = 4, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };

//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalById(rentalId)).ReturnsAsync(rental);
//            this._mockRentalService.Setup(rentalService => rentalService.GetGameName(rentalId)).ReturnsAsync("Catan");
//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(rentalId)).ReturnsAsync(50m);

//            this._mockUserRepository.Setup(userRepository => userRepository.GetById(rental.OwnerId)).ReturnsAsync((User)null);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetById(rental.ClientId)).ReturnsAsync((User)null);

//            var result = await this._cardPaymentService.GetRequestDTO(rentalId);

//            Assert.NotNull(result);
//            Assert.Equal("Unknown Owner", result.OwnerName);
//            Assert.Equal("Unknown Client", result.ClientName);
//            Assert.Equal("Catan", result.GameName);
//        }

//        [Fact]
//        public async Task GetRequestDTO_ValidData_ReturnsFullyPopulatedDTO()
//        {
//            int rentalId = 1;
//            var rental = new Rental { RentalId = rentalId, GameId = 2, OwnerId = 3, ClientId = 4, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
//            var owner = new User { Id = 3, Username = "Alice" };
//            var client = new User { Id = 4, Username = "Bob" };

//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalById(rentalId)).ReturnsAsync(rental);
//            this._mockRentalService.Setup(rentalService => rentalService.GetGameName(rentalId)).ReturnsAsync("Monopoly");
//            this._mockRentalService.Setup(rentalService => rentalService.GetRentalPrice(rentalId)).ReturnsAsync(30m);

//            this._mockUserRepository.Setup(userRepository => userRepository.GetById(rental.OwnerId)).ReturnsAsync(owner);
//            this._mockUserRepository.Setup(userRepository => userRepository.GetById(rental.ClientId)).ReturnsAsync(client);

//            var result = await this._cardPaymentService.GetRequestDTO(rentalId);

//            Assert.NotNull(result);
//            Assert.Equal(rentalId, result.Id);
//            Assert.Equal("Alice", result.OwnerName);
//            Assert.Equal("Bob", result.ClientName);
//            Assert.Equal("Monopoly", result.GameName);
//            Assert.Equal(30m, result.Price);
//        }

//        #endregion
//    }
//}
