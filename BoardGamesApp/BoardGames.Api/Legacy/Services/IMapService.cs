// <copyright file="IMapService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Api.Legacy.Services
{
    public interface IMapService
    {
        /// <summary>
        /// This method does the reverse geocode from coordinates to an address.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>The address from those specific coordinates.</returns>
        public Task<Address?> GetAddressFromMapAsync(double latitude, double longitude);
    }
}
