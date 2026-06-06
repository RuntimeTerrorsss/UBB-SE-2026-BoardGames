// <copyright file="RentalConfirmNavigation.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Desktop.ViewModels
{
    public sealed record RentalConfirmNavigation(GameDetailDTO Game, DateTime StartDate, DateTime EndDate);
}
