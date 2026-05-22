// <copyright file="PagedResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace BoardGames.Shared.DTO
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => TotalCount == 0 ? 0 : CalculateTotalPages();

        private int CalculateTotalPages()
        {
            return (int)Math.Ceiling((double)TotalCount / PageSize);
        }
    }
}
