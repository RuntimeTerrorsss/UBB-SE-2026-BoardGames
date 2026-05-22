// <copyright file="SortOption.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardGames.Data.Enum
{
    /// <summary>
    /// Specifies the sorting options available for game search results.
    /// </summary>
    public enum SortOption
    {
        /// <summary>
        /// No sorting is applied.
        /// </summary>
        None,

        /// <summary>
        /// Sorts results by price in ascending order (lowest to highest).
        /// </summary>
        PriceAscending,

        /// <summary>
        /// Sorts results by price in descending order (highest to lowest).
        /// </summary>
        PriceDescending,

        /// <summary>
        /// Sorts results based on geographical proximity to a specified location.
        /// </summary>
        Location,
    }
}
