// <copyright file="RentalsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly IRentalProxyService rentalProxyService;

        public RentalsController(IRentalProxyService rentalProxyService)
        {
            this.rentalProxyService = rentalProxyService ?? throw new ArgumentNullException(nameof(rentalProxyService));
        }

        public async Task<IActionResult> My()
        {
            Guid renterId = this.User.GetAccountId();
            var rentals = await this.rentalProxyService.GetRentalsForRenterAsync(renterId);
            return this.View(rentals);
        }

        public async Task<IActionResult> Others()
        {
            Guid ownerId = this.User.GetAccountId();
            var rentals = await this.rentalProxyService.GetRentalsForOwnerAsync(ownerId);
            return this.View(rentals);
        }
    }
}
