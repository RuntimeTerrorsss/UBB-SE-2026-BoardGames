// <copyright file="DashboardViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Web.Models.Games;
using BoardGames.Web.Models.Rentals;

namespace BoardGames.Web.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<GameViewModel> MyGames { get; set; } = new List<GameViewModel>();

        public List<RentalViewModel> ActiveRentals { get; set; } = new List<RentalViewModel>();

        public int TotalGamesOwned => this.MyGames.Count;

        public int TotalActiveRentals => this.ActiveRentals.Count;
    }
}
