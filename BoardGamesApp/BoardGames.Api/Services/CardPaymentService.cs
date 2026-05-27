// <copyright file="CardPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Constants;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class CardPaymentService : PaymentService, ICardPaymentService
    {
        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalService;

        public CardPaymentService(
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IReceiptService receiptService,
            IRentalService rentalService)
            : base(paymentRepository, receiptService)
        {
            this.userRepository = userRepository;
            this.rentalService = rentalService;
        }

        public virtual async Task<CardPaymentDTO> AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount)
        {
            if (!await this.CheckBalanceSufficiency(requestIdentifier, clientIdentifier))
            {
                throw new Exception("Insufficient Funds");
            }

            await this.ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier);

            Payment payment = new Payment
            {
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier,
                PaidAmount = amount,
                PaymentMethod = CardPaymentConstants.CardPaymentMethodName,
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null,
                PaymentState = CardPaymentConstants.SuccessfulPaymentState,
                ReceiptFilePath = null,
            };

            payment.TransactionIdentifier = await this.paymentRepository.AddPaymentAsync(payment);
            string receiptFilePath = this.receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.ReceiptFilePath = receiptFilePath;
            await this.paymentRepository.UpdatePaymentAsync(payment);

            return this.ConvertToDTO(payment);
        }

        public async Task<bool> CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier)
        {
            return await this.rentalService.GetRentalPrice(requestIdentifier) <= await this.userRepository.GetUserBalance(clientIdentifier);
        }

        public async Task<CardPaymentDTO?> GetCardPaymentAsync(int paymentIdentifier)
        {
            var payment = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            return payment == null ? null : this.ConvertToDTO(payment);
        }

        public async Task<decimal> GetCurrentBalance(int clientIdentifier)
        {
            return await this.userRepository.GetUserBalance(clientIdentifier);
        }

        public async Task ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            decimal rentalPrice = await this.rentalService.GetRentalPrice(rentalIdentifier);
            decimal clientBalance = await this.userRepository.GetUserBalance(clientIdentifier);
            decimal ownerBalance = await this.userRepository.GetUserBalance(ownerIdentifier);
            decimal newClientBalance = clientBalance - rentalPrice;

            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }

            await this.userRepository.UpdateBalance(clientIdentifier, newClientBalance);
            await this.userRepository.UpdateBalance(ownerIdentifier, ownerBalance + rentalPrice);
        }

        public CardPaymentDTO ConvertToDTO(Payment cardPayment)
        {
            return new CardPaymentDTO(
                    transactionIdentifier: cardPayment.TransactionIdentifier,
                    requestIdentifier: cardPayment.RequestId,
                    clientIdentifier: cardPayment.ClientId,
                    ownerIdentifier: cardPayment.OwnerId,
                    amount: cardPayment.PaidAmount,
                    dateOfTransaction: cardPayment.DateOfTransaction ?? DateTime.Now,
                    paymentMethod: cardPayment.PaymentMethod);
        }

        public virtual async Task<RentalDTO> GetRequestDTO(int rentalIdentifier)
        {
            Rental rental = await this.rentalService.GetRentalById(rentalIdentifier)
                ?? throw new InvalidOperationException($"Rental with ID {rentalIdentifier} was not found.");

            string gameName = await this.rentalService.GetGameName(rental.Id);
            User? ownerUser = await this.userRepository.GetById(rental.OwnerId);
            User? clientUser = await this.userRepository.GetById(rental.ClientId);
            string ownerName = ownerUser?.Username ?? "Unknown Owner";
            string clientName = clientUser?.Username ?? "Unknown Client";
            decimal gamePrice = await this.rentalService.GetRentalPrice(rental.Id);

            return new RentalDTO(rental.Id, rental.GameId, gameName, rental.ClientId, clientName, rental.OwnerId, ownerName, rental.StartDate, rental.EndDate, gamePrice);
        }
    }
}
