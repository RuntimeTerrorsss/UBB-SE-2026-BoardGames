using BoardGames.Web.Models.Rentals;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class RentalsController : BaseController
    {
        private const int NewPaymentPlaceholderId = -1;

        private readonly IRentalService _rentalService;
        private readonly ICardPaymentService _cardPaymentService;
        private readonly ICashPaymentService _cashPaymentService;
        private readonly IConversationService _conversationService;
        private readonly IUserRepository _userRepository;

        public RentalsController(
            IRentalService rentalService,
            ICardPaymentService cardPaymentService,
            ICashPaymentService cashPaymentService,
            IConversationService conversationService,
            IUserRepository userRepository)
        {
            _rentalService = rentalService;
            _cardPaymentService = cardPaymentService;
            _cashPaymentService = cashPaymentService;
            _conversationService = conversationService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int rentalId, int messageId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int userId = CurrentUserId ?? -1;
            RentalDataTransferObject rental;
            try
            {
                rental = await _cardPaymentService.GetRequestDataTransferObject(rentalId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            if (rental.ClientId != userId)
            {
                return Forbid();
            }

            var user = await _userRepository.GetById(userId);

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
            if (redirect != null) return redirect;

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
                await _userRepository.SaveAddress(userId, new Address(
                    model.Country,
                    model.City,
                    model.Street,
                    model.StreetNumber));
            }

            if (string.Equals(model.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(CashPayment), new
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
            if (redirect != null) return redirect;

            RentalDataTransferObject rental;
            try
            {
                rental = await _cardPaymentService.GetRequestDataTransferObject(rentalId);
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
            if (redirect != null) return redirect;

            int userId = CurrentUserId ?? -1;
            var rental = await _cardPaymentService.GetRequestDataTransferObject(rentalId);

            if (rental.ClientId != userId)
            {
                return Forbid();
            }

            try
            {
                decimal rentalPrice = await _rentalService.GetRentalPrice(rentalId);
                await _cashPaymentService.AddCashPaymentAsync(
                    new CashPaymentDTO(NewPaymentPlaceholderId, rentalId, rental.ClientId, rental.OwnerId, rentalPrice));

                _conversationService.Initialize(userId);
                await _conversationService.OnCardPaymentSelected(messageId);

                TempData["Success"] = "Cash payment recorded and added to your payment history.";
                return RedirectToAction("Index", "PaymentHistory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cash payment failed: {ex.Message}";
                return RedirectToAction(nameof(CashPayment), new { rentalId, messageId });
            }
        }
    }
}
