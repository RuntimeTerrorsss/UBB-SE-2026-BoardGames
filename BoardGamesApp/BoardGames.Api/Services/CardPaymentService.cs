// <copyright file="CardPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;

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
            if (!await CheckBalanceSufficiency(requestIdentifier, clientIdentifier))
            {
                throw new Exception("Insufficient Funds");
            }

            await ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier);

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

            payment.TransactionIdentifier = await paymentRepository.AddPaymentAsync(payment);
            string receiptFilePath = receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.ReceiptFilePath = receiptFilePath;
            await paymentRepository.UpdatePaymentAsync(payment);

            return ConvertToDataTransferObject(payment);
        }

        public async Task<bool> CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier)
        {
            return await rentalService.GetRentalPrice(requestIdentifier) <= await userRepository.GetUserBalance(clientIdentifier);
        }

        public async Task<CardPaymentDTO?> GetCardPaymentAsync(int paymentIdentifier)
        {
            var payment = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            return payment == null ? null : ConvertToDataTransferObject(payment);
        }

        public async Task<decimal> GetCurrentBalance(int clientIdentifier)
        {
            return await userRepository.GetUserBalance(clientIdentifier);
        }

        public async Task ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            decimal rentalPrice = await rentalService.GetRentalPrice(rentalIdentifier);
            decimal clientBalance = await userRepository.GetUserBalance(clientIdentifier);
            decimal ownerBalance = await userRepository.GetUserBalance(ownerIdentifier);
            decimal newClientBalance = clientBalance - rentalPrice;

            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }

            await userRepository.UpdateBalance(clientIdentifier, newClientBalance);
            await userRepository.UpdateBalance(ownerIdentifier, ownerBalance + rentalPrice);
        }

        public CardPaymentDTO ConvertToDataTransferObject(Payment cardPayment)
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

        public virtual async Task<RentalDataTransferObject> GetRequestDataTransferObject(int rentalIdentifier)
        {
            Rental rental = await rentalService.GetRentalById(rentalIdentifier)
                ?? throw new InvalidOperationException($"Rental with ID {rentalIdentifier} was not found.");

            string gameName = await rentalService.GetGameName(rental.RentalId);
            User? ownerUser = await userRepository.GetById(rental.OwnerId);
            User? clientUser = await userRepository.GetById(rental.ClientId);
            string ownerName = ownerUser?.Username ?? "Unknown Owner";
            string clientName = clientUser?.Username ?? "Unknown Client";
            decimal gamePrice = await rentalService.GetRentalPrice(rental.RentalId);

            return new RentalDataTransferObject(rental.RentalId, rental.GameId, gameName, rental.ClientId, clientName, rental.OwnerId, ownerName, rental.StartDate, rental.EndDate, gamePrice);
        }
    }
}
