// <copyright file="PagedResult.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => this.TotalCount == 0 ? 0 : this.CalculateTotalPages();

        private int CalculateTotalPages()
        {
            return (int)Math.Ceiling((double)this.TotalCount / this.PageSize);
        }
    }
}
