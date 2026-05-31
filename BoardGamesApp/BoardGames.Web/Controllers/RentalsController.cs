// <copyright file="RentalsController.cs" company="BoardRent">
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
    public class RentalsController : Controller
    {
        private readonly IRentalProxyService rentalProxyService;

        public RentalsController(IRentalProxyService rentalProxyService)
        {
            this.rentalProxyService = rentalProxyService ?? throw new ArgumentNullException(nameof(rentalProxyService));
        }

        public async Task<IActionResult> My()
        {
            Guid renterId = this.User.GetAccountId();
            var rentals = await this.rentalProxyService.GetRentalsForRenterAsync(renterId);
            return this.View(rentals);
        }

        public async Task<IActionResult> Others()
        {
            Guid ownerId = this.User.GetAccountId();
            var rentals = await this.rentalProxyService.GetRentalsForOwnerAsync(ownerId);
            return this.View(rentals);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int rentalId, int requestId, int messageId)
        {
            Guid accountId = this.User.GetAccountId();
            var summary = await this.rentalProxyService.GetCheckoutSummaryAsync(rentalId, accountId);
            if (summary is null)
            {
                return this.NotFound();
            }

            this.ViewBag.RequestId = requestId;
            this.ViewBag.MessageId = messageId;
            return this.View(summary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePayment(
            int rentalId,
            int requestId,
            int messageId,
            string cardNumber,
            string cardholderName,
            string expiryDate,
            string cardVerificationValue)
        {
            try
            {
                await this.rentalProxyService.CompleteCardPaymentAsync(new CompleteRentalCardPaymentDTO
                {
                    RentalId = rentalId,
                    RequestId = requestId,
                    MessageId = messageId,
                    RenterAccountId = this.User.GetAccountId(),
                    CardNumber = cardNumber,
                    CardholderName = cardholderName,
                    ExpiryDate = expiryDate,
                    CardVerificationValue = cardVerificationValue,
                });

                return this.RedirectToAction("Index", "Chats", new { paymentCompleted = true });
            }
            catch (Exception ex)
            {
                this.TempData["PaymentError"] = ex.Message;
                return this.RedirectToAction(nameof(Checkout), new { rentalId, requestId, messageId });
            }
        }
    }
}
