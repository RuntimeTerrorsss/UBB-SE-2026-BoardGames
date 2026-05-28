using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using Moq;
using Xunit;

namespace BoardGames.Tests.Api.Services
{
    public class DashboardServiceTests
    {
        private readonly Mock<IRepositoryPayment> mockPaymentRepository;
        private readonly Mock<IAccountRepository> mockAccountRepository;
        private readonly DashboardService dashboardService;

        public DashboardServiceTests()
        {
            this.mockPaymentRepository = new Mock<IRepositoryPayment>();
            this.mockAccountRepository = new Mock<IAccountRepository>();
            this.dashboardService = new DashboardService(
                this.mockPaymentRepository.Object,
                this.mockAccountRepository.Object);
        }

        [Fact]
        public async Task GetPaymentHistoryForUser_WhenUserNotFound_ReturnsEmptyList()
        {
            var accountId = Guid.NewGuid();

            this.mockAccountRepository
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync((User?)null);

            var result = await this.dashboardService.GetPaymentHistoryForUser(accountId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaymentHistoryForUser_ReturnsPaymentsWhereUserIsRenterOrOwner()
        {
            var accountId = Guid.NewGuid();
            int userPamId = 42;
            var user = new User { Id = accountId, PamUserId = userPamId };

            var payments = new List<HistoryPayment>
            {
                new HistoryPayment { ClientId = userPamId, OwnerId = 99 },
                new HistoryPayment { ClientId = 77, OwnerId = userPamId },
            };

            this.mockAccountRepository
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(user);

            this.mockPaymentRepository
                .Setup(repository => repository.GetAllPayments())
                .ReturnsAsync(payments);

            var result = await this.dashboardService.GetPaymentHistoryForUser(accountId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetPaymentHistoryForUser_ExcludesPaymentsForOtherUsers()
        {
            var accountId = Guid.NewGuid();
            int userPamId = 42;
            var user = new User { Id = accountId, PamUserId = userPamId };

            var payments = new List<HistoryPayment>
            {
                new HistoryPayment { ClientId = 11, OwnerId = 22 },
                new HistoryPayment { ClientId = userPamId, OwnerId = 22 },
            };

            this.mockAccountRepository
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(user);

            this.mockPaymentRepository
                .Setup(repository => repository.GetAllPayments())
                .ReturnsAsync(payments);

            var result = await this.dashboardService.GetPaymentHistoryForUser(accountId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetPaymentHistoryForUser_OrdersResultsByMostRecentDateFirst()
        {
            var accountId = Guid.NewGuid();
            int userPamId = 42;
            var user = new User { Id = accountId, PamUserId = userPamId };

            var olderPayment = new HistoryPayment
            {
                ClientId = userPamId,
                OwnerId = 99,
                RentalStartDate = new DateTime(2026, 1, 1),
                RentalEndDate = new DateTime(2026, 1, 5),
            };

            var newerPayment = new HistoryPayment
            {
                ClientId = userPamId,
                OwnerId = 99,
                RentalStartDate = new DateTime(2026, 6, 1),
                RentalEndDate = new DateTime(2026, 6, 5),
            };

            this.mockAccountRepository
                .Setup(repository => repository.GetByIdAsync(accountId))
                .ReturnsAsync(user);

            this.mockPaymentRepository
                .Setup(repository => repository.GetAllPayments())
                .ReturnsAsync(new List<HistoryPayment> { olderPayment, newerPayment });

            var result = await this.dashboardService.GetPaymentHistoryForUser(accountId);

            Assert.Equal(new DateTime(2026, 6, 1), result[0].SortDate);
            Assert.Equal(new DateTime(2026, 1, 1), result[1].SortDate);
        }
    }
}
