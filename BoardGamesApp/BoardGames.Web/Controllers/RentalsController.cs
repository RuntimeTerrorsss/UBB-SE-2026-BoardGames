// <copyright file="RentalsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Web.Models.Rentals;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class RentalsController : BaseController
    {
        private const int NewPaymentPlaceholderId = -1;

        private readonly IRentalService rentalService;
        private readonly ICardPaymentService cardPaymentService;
        private readonly ICashPaymentService cashPaymentService;
        private readonly IConversationService conversationService;
        private readonly IUserRepository userRepository;

        public RentalsController(
            IRentalService rentalService,
            ICardPaymentService cardPaymentService,
            ICashPaymentService cashPaymentService,
            IConversationService conversationService,
            IUserRepository userRepository)
        {
            this.rentalService = rentalService;
            this.cardPaymentService = cardPaymentService;
            this.cashPaymentService = cashPaymentService;
            this.conversationService = conversationService;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int rentalId, int messageId)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            int userId = CurrentUserId ?? -1;
            RentalDTO rental;
            try
            {
                rental = await this.cardPaymentService.GetRequestDTO(rentalId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            if (rental.ClientId != userId)
            {
                return Forbid();
            }

            var user = await this.userRepository.GetById(userId);

            var model = new RentalCheckoutViewModel
            {
                RentalId = rental.Id,
                MessageId = messageId,
                GameId = rental.GameId,
                GameName = rental.GameName,
                OwnerName = rental.OwnerName,
                ClientId = rental.ClientId,
                OwnerId = rental.OwnerId,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                TotalPrice = rental.Price,
                Country = user?.Country ?? string.Empty,
                City = user?.City ?? string.Empty,
                Street = user?.Street ?? string.Empty,
                StreetNumber = user?.StreetNumber ?? string.Empty,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(RentalCheckoutViewModel model)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            int userId = CurrentUserId ?? -1;
            if (model.ClientId != userId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SaveAddress)
            {
                await this.userRepository.SaveAddress(userId, new Address(
                    model.Country,
                    model.City,
                    model.Street,
                    model.StreetNumber));
            }

            if (string.Equals(model.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(this.CashPayment), new
                {
                    rentalId = model.RentalId,
                    messageId = model.MessageId,
                });
            }

            return RedirectToAction("CardPayment", "Payment", new
            {
                requestIdentifier = model.RentalId,
                clientIdentifier = model.ClientId,
                ownerIdentifier = model.OwnerId,
                messageId = model.MessageId,
            });
        }

        [HttpGet]
        public async Task<IActionResult> CashPayment(int rentalId, int messageId)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            RentalDTO rental;
            try
            {
                rental = await this.cardPaymentService.GetRequestDTO(rentalId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            if (rental.ClientId != (CurrentUserId ?? -1))
            {
                return Forbid();
            }

            ViewBag.Rental = rental;
            ViewBag.MessageId = messageId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCashPayment(int rentalId, int messageId)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            int userId = CurrentUserId ?? -1;
            var rental = await this.cardPaymentService.GetRequestDTO(rentalId);

            if (rental.ClientId != userId)
            {
                return Forbid();
            }

            try
            {
                decimal rentalPrice = await this.rentalService.GetRentalPrice(rentalId);
                await this.cashPaymentService.AddCashPaymentAsync(
                    new CashPaymentDTO(NewPaymentPlaceholderId, rentalId, rental.ClientId, rental.OwnerId, rentalPrice));

                this.conversationService.Initialize(userId);
                await this.conversationService.OnCardPaymentSelected(messageId);

                TempData["Success"] = "Cash payment recorded and added to your payment history.";
                return RedirectToAction("Index", "PaymentHistory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cash payment failed: {ex.Message}";
                return RedirectToAction(nameof(this.CashPayment), new { rentalId, messageId });
            }
        }
    }
}
