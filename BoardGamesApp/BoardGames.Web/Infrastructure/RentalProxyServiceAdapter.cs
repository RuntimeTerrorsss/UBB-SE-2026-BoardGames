using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Web.Infrastructure;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using GUI_BRAP.ProxyServices;

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
            => (await rentalService.GetRentalsForOwnerAsync(ownerAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await rentalService.GetRentalsForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();
    }
}
