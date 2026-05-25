using System;
using System.Threading.Tasks;
using BoardGames.Web.Models.Payment;
using BoardGames.Shared.ProxyServices;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly ICardPaymentService _cardPaymentService;
        private readonly IConversationService _conversationService;

        public PaymentController(ICardPaymentService cardPaymentService, IConversationService conversationService)
        {
            _cardPaymentService = cardPaymentService;
            _conversationService = conversationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            return RedirectToAction(nameof(CardPayment));
        }

        [HttpGet]
        public async Task<IActionResult> CardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, int messageId = 0)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            if ((CurrentUserId ?? -1) != clientIdentifier)
            {
                return Forbid();
            }

            var rental = await _cardPaymentService.GetRequestDataTransferObject(requestIdentifier);
            decimal balance = await _cardPaymentService.GetCurrentBalance(clientIdentifier);

            return View(new PaymentViewModel
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
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            if ((CurrentUserId ?? -1) != model.ClientIdentifier)
            {
                return Forbid();
            }

            model.PaymentMethod = "Card";

            if (model.RequestIdentifier <= 0)
            {
                ModelState.AddModelError(nameof(model.RequestIdentifier), "Invalid request identifier.");
            }

            if (model.ClientIdentifier <= 0)
            {
                ModelState.AddModelError(nameof(model.ClientIdentifier), "Invalid client identifier.");
            }

            if (model.OwnerIdentifier <= 0)
            {
                ModelState.AddModelError(nameof(model.OwnerIdentifier), "Invalid owner identifier.");
            }

            if (!ModelState.IsValid)
            {
                model.CardNumber = string.Empty;
                model.Cvv = string.Empty;
                model.CardholderName = string.Empty;
                model.Expiry = string.Empty;
                model.AccountBalance = await _cardPaymentService.GetCurrentBalance(model.ClientIdentifier);
                return View(model);
            }

            try
            {
                var result = await _cardPaymentService.AddCardPayment(
                    model.RequestIdentifier,
                    model.ClientIdentifier,
                    model.OwnerIdentifier,
                    model.Amount);

                if (model.MessageId > 0)
                {
                    _conversationService.Initialize(model.ClientIdentifier);
                    await _conversationService.OnCardPaymentSelected(model.MessageId);
                }

                TempData["Success"] = $"Payment successful! Transaction ID: {result.TransactionIdentifier}";
                return RedirectToAction("Index", "PaymentHistory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AccountBalance = await _cardPaymentService.GetCurrentBalance(model.ClientIdentifier);
                return View(model);
            }
        }
    }
}
