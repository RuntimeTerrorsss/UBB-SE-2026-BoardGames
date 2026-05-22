// <copyright file="ICardPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BookingBoardGames.Data;

namespace BoardGames.Shared.Services
{
    public interface ICardPaymentService
    {
        Task<CardPaymentDTO> AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount);

        Task<bool> CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier);

        Task<CardPaymentDTO?> GetCardPaymentAsync(int paymentIdentifier);

        Task<decimal> GetCurrentBalance(int clientIdentifier);

        Task ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier);

        CardPaymentDTO ConvertToDataTransferObject(Payment cardPayment);

        Task<RentalDataTransferObject> GetRequestDataTransferObject(int rentalIdentifier);
    }
}
