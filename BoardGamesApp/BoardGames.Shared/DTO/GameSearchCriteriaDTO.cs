// <copyright file="GameSearchCriteriaDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;

namespace BoardGames.Shared.DTO
{
    public class GameSearchCriteriaDTO
    {
        public string? Name { get; set; }

        public string? City { get; set; }

        public decimal? MaximumPrice { get; set; }

        public int? PlayerCount { get; set; }

        public DateTime? AvailableFrom { get; set; }

        public DateTime? AvailableTo { get; set; }

        public string? SortBy { get; set; }
    }
}
