// <copyright file="DashboardViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Web.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<RentalDTO> UpcomingRentals { get; set; } = new();

        public List<RequestDTO> OpenRequests { get; set; } = new();

        public List<PaymentDTO> RecentPayments { get; set; } = new();
    }
}
