using BoardGames.Shared.DTO;
using System.Collections.Immutable;

namespace BoardGames.Api.Services
{
    public interface IRentalService
    {
        ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId);
        ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId);
        bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate);
        void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate);
    }
}
