using System;
using System.Collections.Immutable;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IRentalService
    {
        ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId);
        ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId);
        bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate);
        void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate);
        Task<Rental?> GetRentalById(int rentalId);
        Task<decimal> GetRentalPrice(int rentalId);
        Task<string> GetGameName(int rentalId);
        Task<List<RentalDTO>> GetRentalsForUser(int userId);
    }
}
