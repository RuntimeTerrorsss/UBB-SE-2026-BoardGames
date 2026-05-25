// <copyright file="RentalProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class RentalProxyServiceAdapter : IRentalProxyService
    {
        private readonly IRentalService rentalService;

        public RentalProxyServiceAdapter(IRentalService rentalService)
        {
            this.rentalService = rentalService;
        }

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => (await this.rentalService.GetRentalsForOwnerAsync(ownerAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await this.rentalService.GetRentalsForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();
    }
}
