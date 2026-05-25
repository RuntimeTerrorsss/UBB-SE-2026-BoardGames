using System;
using System.Collections.Immutable;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IBoardRentRentalService
    {
        ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId);
        ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId);
        bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate);
        void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate);
    }
}
