using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IRentalService
    {
        Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> IsSlotAvailableAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task<ServiceResult> CreateConfirmedRentalAsync(CreateRentalDataTransferObject rental, CancellationToken cancellationToken = default);
    }
}
