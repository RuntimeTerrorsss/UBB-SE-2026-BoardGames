using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Enums;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class PaymentHistoryViewModelTests
    {
        private Mock<IPaymentService> mockPaymentService = null!;
        private Mock<ISessionContext> mockSessionContext = null!;
        private PaymentHistoryViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.mockPaymentService = new Mock<IPaymentService>();
            this.mockSessionContext = new Mock<ISessionContext>();
            this.mockSessionContext.Setup(s => s.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(s => s.AccountId).Returns(Guid.NewGuid());

            this.viewModel = new PaymentHistoryViewModel(this.mockPaymentService.Object, this.mockSessionContext.Object);
        }

        [Test]
        public async Task ApplyFilter_ShouldLoadPayments_WhenServiceReturnsSuccess()
        {
            var payments = new List<PaymentDTO> { new PaymentDTO { PaymentId = 1, Amount = 50 } };

            var pagedResult = new PagedResult<PaymentDTO>
            {
                Items = payments,
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            this.mockPaymentService
                .Setup(s => s.GetFilteredPaymentsAsync(It.IsAny<Guid>(), It.IsAny<FilterType>(), It.IsAny<PaymentMethod>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(ServiceResult<PagedResult<PaymentDTO>>.Ok(pagedResult));

            await this.viewModel.ApplyFilter(true);

            Assert.That(this.viewModel.Payments.Count, Is.EqualTo(1));
            Assert.That(this.viewModel.TotalAmount, Is.EqualTo(50));
            Assert.That(this.viewModel.IsLoading, Is.False);
        }

        [Test]
        public async Task SearchText_ShouldTriggerDebouncedSearch()
        {
            var emptyResult = new PagedResult<PaymentDTO> { Items = new List<PaymentDTO>(), TotalCount = 0, PageNumber = 1, PageSize = 10 };

            this.mockPaymentService
                .Setup(s => s.GetFilteredPaymentsAsync(It.IsAny<Guid>(), It.IsAny<FilterType>(), It.IsAny<PaymentMethod>(), "test", It.IsAny<int>()))
                .ReturnsAsync(ServiceResult<PagedResult<PaymentDTO>>.Ok(emptyResult));

            this.viewModel.SearchText = "test";

            await Task.Delay(400);

            this.mockPaymentService.Verify(s => s.GetFilteredPaymentsAsync(It.IsAny<Guid>(), It.IsAny<FilterType>(), It.IsAny<PaymentMethod>(), "test", It.IsAny<int>()), Times.Once);
        }

        [Test]
        public async Task NextPage_ShouldIncrementPageAndReload()
        {
            this.viewModel.TotalPages = 2;
            var result = new PagedResult<PaymentDTO> { Items = new List<PaymentDTO>(), TotalCount = 2, PageNumber = 2, PageSize = 10 };

            this.mockPaymentService
                .Setup(s => s.GetFilteredPaymentsAsync(It.IsAny<Guid>(), It.IsAny<FilterType>(), It.IsAny<PaymentMethod>(), It.IsAny<string>(), 2))
                .ReturnsAsync(ServiceResult<PagedResult<PaymentDTO>>.Ok(result));

            this.viewModel.NextPageCommand.Execute(null);

            Assert.That(this.viewModel.CurrentPage, Is.EqualTo(2));
            this.mockPaymentService.Verify(s => s.GetFilteredPaymentsAsync(It.IsAny<Guid>(), It.IsAny<FilterType>(), It.IsAny<PaymentMethod>(), It.IsAny<string>(), 2), Times.Once);
        }
    }
}