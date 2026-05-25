// <copyright file="CashPaymentViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.ViewModels
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
            DeliveryAddress = deliveryAddress;
        }

        public async Task InitializeAsync(int rentalRequestId, string deliveryAddress)
        {
            Rental rentalRequest = await rentalRequestService.GetRentalById(rentalRequestId)
                ?? throw new InvalidOperationException($"Rental with ID {rentalRequestId} was not found.");

            Game game = await gameRepository.GetGameById(rentalRequest.GameId)
                ?? throw new InvalidOperationException($"Game with ID {rentalRequest.GameId} was not found.");

            User clientUser = await userRepository.GetById(rentalRequest.ClientId)
                ?? throw new InvalidOperationException($"Client user with ID {rentalRequest.ClientId} was not found.");

            User ownerUser = await userRepository.GetById(rentalRequest.OwnerId)
                ?? throw new InvalidOperationException($"Owner user with ID {rentalRequest.OwnerId} was not found.");

            OwnerName = ownerUser.Username;
            GameName = game.Name;
            RequestDates = rentalRequest.StartDate.ToShortDateString() + DateRangeSeparator + rentalRequest.EndDate.ToShortDateString();
            DeliveryAddress = deliveryAddress;

            decimal rentalPrice = await rentalRequestService.GetRentalPrice(rentalRequestId);
            PaidAmount = rentalPrice.ToString();

            int createdPaymentIdentifier = await cashPaymentService.AddCashPaymentAsync(
                new CashPaymentDataTransferObject(NewPaymentPlaceholderId, rentalRequestId, clientUser.Id, ownerUser.Id, rentalPrice));

            await conversationService.OnCashPaymentSelected(rentalRequestMessageIdentifier, createdPaymentIdentifier);
        }
    }
}
