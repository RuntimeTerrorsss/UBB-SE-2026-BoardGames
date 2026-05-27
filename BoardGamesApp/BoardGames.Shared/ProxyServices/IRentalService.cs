// <copyright file="IRentalService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IRentalService
    {
        Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> IsSlotAvailableAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task<ServiceResult> CreateConfirmedRentalAsync(CreateRentalDTO rental, CancellationToken cancellationToken = default);
    }
}
