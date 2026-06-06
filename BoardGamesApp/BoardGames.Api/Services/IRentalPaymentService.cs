// <copyright file="IRentalPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IRentalPaymentService
    {
        Task<RentalCheckoutDTO?> GetCheckoutSummaryAsync(int rentalId, Guid renterAccountId);

        Task CompleteCardPaymentAsync(CompleteRentalCardPaymentDTO payment);
    }
}
