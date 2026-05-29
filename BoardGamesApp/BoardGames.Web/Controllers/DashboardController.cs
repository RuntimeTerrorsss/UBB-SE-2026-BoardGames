// <copyright file="DashboardController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IRentalProxyService rentalProxyService;
        private readonly IRequestProxyService requestProxyService;
        private readonly IPaymentProxyService paymentProxyService;

        public DashboardController(
            IRentalProxyService rentalProxyService,
            IRequestProxyService requestProxyService,
            IPaymentProxyService paymentProxyService)
        {
            this.rentalProxyService = rentalProxyService;
            this.requestProxyService = requestProxyService;
            this.paymentProxyService = paymentProxyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Guid accountId = this.User.GetAccountId();

            var rentalsTask = this.rentalProxyService.GetRentalsForRenterAsync(accountId);
            var requestsTask = this.requestProxyService.GetOpenRequestsForOwnerAsync(accountId);
            var paymentsTask = this.SafeGetPaymentsAsync(accountId);

            await Task.WhenAll(rentalsTask, requestsTask, paymentsTask);

            var upcomingRentals = rentalsTask.Result
                .Where(r => !r.IsExpired)
                .OrderBy(r => r.StartDate)
                .ToList();

            var model = ViewModelAdapter.ToDashboardViewModel(
                upcomingRentals,
                requestsTask.Result,
                paymentsTask.Result);

            return this.View(model);
        }

        private async Task<IReadOnlyList<PaymentDTO>> SafeGetPaymentsAsync(Guid accountId)
        {
            try
            {
                return await this.paymentProxyService.GetPaymentHistoryForUserAsync(accountId);
            }
            catch (ProxyServiceException)
            {
                return Array.Empty<PaymentDTO>();
            }
        }
    }
}
