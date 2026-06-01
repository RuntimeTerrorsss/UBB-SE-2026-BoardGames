// <copyright file="DashboardControllerTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BoardGames.Web.Controllers;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class DashboardControllerTests
    {
        private readonly Mock<IRentalProxyService> rentalProxy;
        private readonly Mock<IRequestProxyService> requestProxy;
        private readonly Mock<IPaymentProxyService> paymentProxy;
        private readonly Guid accountId;

        public DashboardControllerTests()
        {
            this.rentalProxy = new Mock<IRentalProxyService>();
            this.requestProxy = new Mock<IRequestProxyService>();
            this.paymentProxy = new Mock<IPaymentProxyService>();
            this.accountId = Guid.NewGuid();
        }

        private DashboardController CreateController()
        {
            var controller = new DashboardController(
                this.rentalProxy.Object,
                this.requestProxy.Object,
                this.paymentProxy.Object);

            var identity = new ClaimsIdentity(
                new[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.accountId.ToString()),
            }, "Test");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity),
                },
            };

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewWithDashboardViewModel()
        {
            var rentals = new List<RentalDTO>
            {
                new RentalDTO { Id = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3) },
            };
            var requests = new List<RequestDTO>
            {
                new RequestDTO { Id = 1 },
            };
            var payments = new List<PaymentDTO>
            {
                new PaymentDTO { PaymentId = 1, Amount = 50m },
            };

            this.rentalProxy
                .Setup(s => s.GetRentalsForRenterAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rentals);
            this.requestProxy
                .Setup(s => s.GetOpenRequestsForOwnerAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requests);
            this.paymentProxy
                .Setup(s => s.GetPaymentHistoryForUserAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            var controller = this.CreateController();

            var result = await controller.Index() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<DashboardViewModel>(result!.Model);
            Assert.Single(model.UpcomingRentals);
            Assert.Single(model.OpenRequests);
            Assert.Single(model.RecentPayments);
        }

        [Fact]
        public async Task Index_FiltersExpiredRentals()
        {
            var rentals = new List<RentalDTO>
            {
                new RentalDTO { Id = 1, StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddDays(-5) },
                new RentalDTO { Id = 2, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3) },
            };

            this.rentalProxy
                .Setup(s => s.GetRentalsForRenterAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rentals);
            this.requestProxy
                .Setup(s => s.GetOpenRequestsForOwnerAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RequestDTO>());
            this.paymentProxy
                .Setup(s => s.GetPaymentHistoryForUserAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PaymentDTO>());

            var controller = this.CreateController();

            var result = await controller.Index() as ViewResult;

            var model = Assert.IsType<DashboardViewModel>(result!.Model);
            Assert.Single(model.UpcomingRentals);
            Assert.Equal(2, model.UpcomingRentals[0].Id);
        }

        [Fact]
        public async Task Index_PaymentProxyFails_StillReturnsView()
        {
            this.rentalProxy
                .Setup(s => s.GetRentalsForRenterAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RentalDTO>());
            this.requestProxy
                .Setup(s => s.GetOpenRequestsForOwnerAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RequestDTO>());
            this.paymentProxy
                .Setup(s => s.GetPaymentHistoryForUserAsync(this.accountId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProxyServiceException("fail", HttpStatusCode.InternalServerError, null));

            var controller = this.CreateController();

            var result = await controller.Index() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<DashboardViewModel>(result!.Model);
            Assert.Empty(model.RecentPayments);
        }
    }
}
