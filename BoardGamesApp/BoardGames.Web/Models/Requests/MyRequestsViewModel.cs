// <copyright file="MyRequestsViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Web.Models.Requests
{
    public class MyRequestsViewModel
    {
        public IReadOnlyList<RequestDTO> Requests { get; init; } = new List<RequestDTO>();

        public string? ErrorMessage { get; init; }
    }
}
