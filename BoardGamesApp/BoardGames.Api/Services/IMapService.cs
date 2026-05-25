// <copyright file="IMapService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Api.Services
{
    public interface IMapService
    {
        /// <summary>
        /// This method does the reverse geocode from coordinates to an address
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>The address from those specific coordinates</returns>
        public Task<Address?> GetAddressFromMapAsync(double latitude, double longitude);
    }
}
