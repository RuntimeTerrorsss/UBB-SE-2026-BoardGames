// <copyright file="ViewModelAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Models.Account;
using BoardGames.Web.Models.Dashboard;

namespace BoardGames.Web.Helpers
{
    public static class ViewModelAdapter
    {
        public static ProfileViewModel ToProfileViewModel(AccountProfileDTO profile)
        {
            return new ProfileViewModel
            {
                Username = profile.Username,
                DisplayName = profile.DisplayName ?? string.Empty,
                Email = profile.Email ?? string.Empty,
                PhoneNumber = profile.PhoneNumber,
                Country = profile.Country,
                City = profile.City,
                StreetName = profile.StreetName,
                StreetNumber = profile.StreetNumber,
                AvatarUrl = profile.AvatarUrl ?? string.Empty,
            };
        }

        public static AccountProfileDTO ToAccountProfileDTO(ProfileViewModel model)
        {
            return new AccountProfileDTO
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Country = model.Country,
                City = model.City,
                StreetName = model.StreetName,
                StreetNumber = model.StreetNumber,
            };
        }

        public static DashboardViewModel ToDashboardViewModel(
            IEnumerable<RentalDTO> upcomingRentals,
            IEnumerable<RequestDTO> openRequests,
            IEnumerable<PaymentDTO> recentPayments)
        {
            return new DashboardViewModel
            {
                UpcomingRentals = upcomingRentals.ToList(),
                OpenRequests = openRequests.ToList(),
                RecentPayments = recentPayments.ToList(),
            };
        }
    }
}
