// <copyright file="CashPaymentViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;

namespace BookingBoardGames.Src.ViewModels
{
    public class CashPaymentViewModel
    {
        private const int NewPaymentPlaceholderId = -1;
        private const string DateRangeSeparator = " to ";

        private readonly ICashPaymentService cashPaymentService;
        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalRequestService;
        private readonly InterfaceGamesRepository gameRepository;
        private readonly ConversationService conversationService;
        private readonly int rentalRequestMessageIdentifier;

        public string OwnerName { get; set; }

        public string GameName { get; set; }

        public string DeliveryAddress { get; set; }

        public string RequestDates { get; set; }

        public string PaidAmount { get; set; }

        public CashPaymentViewModel(
            ICashPaymentService cashPaymentService,
            IUserRepository userRepository,
            IRentalService rentalRequestService,
            InterfaceGamesRepository gameRepository,
            int rentalRequestId,
            string deliveryAddress,
            int rentalRequestMessageIdentifier,
            ConversationService conversationService)
        {
            this.cashPaymentService = cashPaymentService;
            this.userRepository = userRepository;
            this.rentalRequestService = rentalRequestService;
            this.gameRepository = gameRepository;
            this.conversationService = conversationService;
            this.rentalRequestMessageIdentifier = rentalRequestMessageIdentifier;
            this.DeliveryAddress = deliveryAddress;
        }

        public async Task InitializeAsync(int rentalRequestId, string deliveryAddress)
        {
            Rental rentalRequest = await this.rentalRequestService.GetRentalById(rentalRequestId)
                ?? throw new InvalidOperationException($"Rental with ID {rentalRequestId} was not found.");

            Game game = await this.gameRepository.GetGameById(rentalRequest.GameId)
                ?? throw new InvalidOperationException($"Game with ID {rentalRequest.GameId} was not found.");

            User clientUser = await this.userRepository.GetById(rentalRequest.ClientId)
                ?? throw new InvalidOperationException($"Client user with ID {rentalRequest.ClientId} was not found.");

            User ownerUser = await this.userRepository.GetById(rentalRequest.OwnerId)
                ?? throw new InvalidOperationException($"Owner user with ID {rentalRequest.OwnerId} was not found.");

            this.OwnerName = ownerUser.Username;
            this.GameName = game.Name;
            this.RequestDates = rentalRequest.StartDate.ToShortDateString() + DateRangeSeparator + rentalRequest.EndDate.ToShortDateString();
            this.DeliveryAddress = deliveryAddress;

            decimal rentalPrice = await this.rentalRequestService.GetRentalPrice(rentalRequestId);
            this.PaidAmount = rentalPrice.ToString();

            int createdPaymentIdentifier = await this.cashPaymentService.AddCashPaymentAsync(
                new CashPaymentDataTransferObject(NewPaymentPlaceholderId, rentalRequestId, clientUser.Id, ownerUser.Id, rentalPrice));

            await this.conversationService.OnCashPaymentSelected(this.rentalRequestMessageIdentifier, createdPaymentIdentifier);
        }
    }
}
