// <copyright file="RentalPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Constants;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class RentalPaymentService : IRentalPaymentService
    {
        private const int MinimumRentalDays = 1;

        private readonly IRentalRepository rentalRepository;
        private readonly InterfaceGamesRepository gamesRepository;
        private readonly IUserRepository userRepository;
        private readonly IAccountRepository accountRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IConversationRepository conversationRepository;
        private readonly IConversationApiService conversationApiService;

        public RentalPaymentService(
            IRentalRepository rentalRepository,
            InterfaceGamesRepository gamesRepository,
            IUserRepository userRepository,
            IAccountRepository accountRepository,
            IPaymentRepository paymentRepository,
            IConversationRepository conversationRepository,
            IConversationApiService conversationApiService)
        {
            this.rentalRepository = rentalRepository;
            this.gamesRepository = gamesRepository;
            this.userRepository = userRepository;
            this.accountRepository = accountRepository;
            this.paymentRepository = paymentRepository;
            this.conversationRepository = conversationRepository;
            this.conversationApiService = conversationApiService;
        }

        public async Task<RentalCheckoutDTO?> GetCheckoutSummaryAsync(int rentalId, Guid renterAccountId)
        {
            var rental = await this.rentalRepository.GetById(rentalId);
            if (rental is null)
            {
                return null;
            }

            var renterPamId = await this.GetPamUserIdAsync(renterAccountId);
            if (rental.ClientId != renterPamId)
            {
                return null;
            }

            var game = await this.gamesRepository.GetGameById(rental.GameId);
            var owner = await this.userRepository.GetById(rental.OwnerId);
            decimal price = await this.CalculateRentalPriceAsync(rental);

            return new RentalCheckoutDTO
            {
                RentalId = rentalId,
                GameName = game?.Name ?? "Game",
                OwnerName = owner?.Username ?? "Owner",
                DateRange = $"{rental.StartDate:d} – {rental.EndDate:d}",
                Price = price,
                Balance = 0,
            };
        }

        public async Task CompleteCardPaymentAsync(CompleteRentalCardPaymentDTO payment)
        {
            bool isCardPayment = !string.Equals(payment.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase);
            if (isCardPayment)
            {
                ValidateCardDetails(payment);
            }

            var rental = await this.rentalRepository.GetById(payment.RentalId)
                ?? throw new InvalidOperationException("Rental not found.");

            int renterPamId = await this.GetPamUserIdAsync(payment.RenterAccountId);
            if (rental.ClientId != renterPamId)
            {
                throw new UnauthorizedAccessException("Only the renter can complete payment for this rental.");
            }

            var rentalMessage = await this.conversationRepository.GetRentalRequestMessageById(payment.MessageId)
                ?? await this.conversationRepository.FindRentalRequestMessageByRequestId(payment.RequestId);
            if (rentalMessage is null)
            {
                throw new InvalidOperationException("Rental request message not found.");
            }

            if (!rentalMessage.IsRequestAccepted || rentalMessage.IsRequestResolved)
            {
                throw new InvalidOperationException("This rental request is not awaiting payment.");
            }

            int linkedRentalId = rentalMessage.RentalRequestId ?? 0;
            if (linkedRentalId > 0 && linkedRentalId != payment.RentalId)
            {
                throw new InvalidOperationException("Rental id does not match the accepted request.");
            }

            decimal price = await this.CalculateRentalPriceAsync(rental);

            var paymentRecord = new Payment
            {
                RequestId = payment.RentalId,
                ClientId = renterPamId,
                OwnerId = rental.OwnerId,
                PaidAmount = price,
                PaymentMethod = isCardPayment ? CardPaymentConstants.CardPaymentMethodName : "CASH",
                DateOfTransaction = DateTime.UtcNow,
                DateConfirmedBuyer = DateTime.UtcNow,
                PaymentState = CardPaymentConstants.SuccessfulPaymentState,
            };

            await this.paymentRepository.AddPaymentAsync(paymentRecord);
            await this.conversationRepository.FinalizeRentalRequestByMessageId(rentalMessage.MessageId, accepted: true);
        }

        private static void ValidateCardDetails(CompleteRentalCardPaymentDTO payment)
        {
            string digitsOnly = new string((payment.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digitsOnly.Length < 13)
            {
                throw new InvalidOperationException("Enter a valid card number.");
            }

            if (string.IsNullOrWhiteSpace(payment.CardholderName))
            {
                throw new InvalidOperationException("Enter the cardholder name.");
            }

            if (string.IsNullOrWhiteSpace(payment.ExpiryDate) || payment.ExpiryDate.Length < 4)
            {
                throw new InvalidOperationException("Enter a valid expiry date (MM/YY).");
            }

            string cvvDigits = new string((payment.CardVerificationValue ?? string.Empty).Where(char.IsDigit).ToArray());
            if (cvvDigits.Length is < 3 or > 4)
            {
                throw new InvalidOperationException("Enter a valid CVV.");
            }
        }

        private async Task<int> GetPamUserIdAsync(Guid accountId)
        {
            var account = await this.accountRepository.GetByIdAsync(accountId);
            return account?.PamUserId ?? throw new KeyNotFoundException($"Account {accountId} not found.");
        }

        private async Task<decimal> CalculateRentalPriceAsync(Rental rental)
        {
            if (rental.TotalPrice is > 0)
            {
                return rental.TotalPrice.Value;
            }

            decimal pricePerDay = await this.gamesRepository.GetPriceGameById(rental.GameId);
            int days = (rental.EndDate.Date - rental.StartDate.Date).Days + MinimumRentalDays;
            if (days < MinimumRentalDays)
            {
                days = MinimumRentalDays;
            }

            return days * pricePerDay;
        }
    }
}
