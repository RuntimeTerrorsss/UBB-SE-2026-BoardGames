// <copyright file="PaymentController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Web.Models.Payment;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly ICardPaymentService cardPaymentService;
        private readonly IConversationService conversationService;

        public PaymentController(ICardPaymentService cardPaymentServiceParam, IConversationService conversationServiceParam)
        {
            this.cardPaymentService = cardPaymentServiceParam;
            this.conversationService = conversationServiceParam;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            return this.RedirectToAction(nameof(this.CardPayment));
        }

        [HttpGet]
        public async Task<IActionResult> CardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, int messageId = 0)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            if ((this.CurrentUserId ?? -1) != clientIdentifier)
            {
                return this.Forbid();
            }

            var rental = await this.cardPaymentService.GetRequestDTO(requestIdentifier);
            decimal balance = await this.cardPaymentService.GetCurrentBalance(clientIdentifier);

            return this.View(new PaymentViewModel
            {
                RequestIdentifier = requestIdentifier,
                ClientIdentifier = clientIdentifier,
                OwnerIdentifier = ownerIdentifier,
                MessageId = messageId,
                Amount = rental.Price,
                PaymentMethod = "Card",
                DateOfTransaction = DateTime.Now,
                GameName = rental.GameName,
                OwnerName = rental.OwnerName,
                RentalPeriod = $"{rental.StartDate:dd MMM yyyy} – {rental.EndDate:dd MMM yyyy}",
                AccountBalance = balance,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CardPayment(PaymentViewModel model)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            if ((this.CurrentUserId ?? -1) != model.ClientIdentifier)
            {
                return this.Forbid();
            }

            model.PaymentMethod = "Card";

            if (model.RequestIdentifier <= 0)
            {
                this.ModelState.AddModelError(nameof(model.RequestIdentifier), "Invalid request identifier.");
            }

            if (model.ClientIdentifier <= 0)
            {
                this.ModelState.AddModelError(nameof(model.ClientIdentifier), "Invalid client identifier.");
            }

            if (model.OwnerIdentifier <= 0)
            {
                this.ModelState.AddModelError(nameof(model.OwnerIdentifier), "Invalid owner identifier.");
            }

            if (!this.ModelState.IsValid)
            {
                model.CardNumber = string.Empty;
                model.Cvv = string.Empty;
                model.CardholderName = string.Empty;
                model.Expiry = string.Empty;
                model.AccountBalance = await this.cardPaymentService.GetCurrentBalance(model.ClientIdentifier);
                return this.View(model);
            }

            try
            {
                var result = await this.cardPaymentService.AddCardPayment(
                    model.RequestIdentifier,
                    model.ClientIdentifier,
                    model.OwnerIdentifier,
                    model.Amount);

                if (model.MessageId > 0)
                {
                    this.conversationService.Initialize(model.ClientIdentifier);
                    await this.conversationService.OnCardPaymentSelected(model.MessageId);
                }

                this.TempData["Success"] = $"Payment successful! Transaction ID: {result.TransactionIdentifier}";
                return this.RedirectToAction("Index", "PaymentHistory");
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
                model.AccountBalance = await this.cardPaymentService.GetCurrentBalance(model.ClientIdentifier);
                return this.View(model);
            }
        }
    }
}
