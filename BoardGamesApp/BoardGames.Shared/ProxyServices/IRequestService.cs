using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IRequestService
    {
        Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> CreateRequestAsync(CreateRequestDataTransferObject request, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> ApproveRequestAsync(int requestId, Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> DenyRequestAsync(int requestId, RequestActionDataTransferObject action, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> CancelRequestAsync(int requestId, RequestActionDataTransferObject action, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> OfferGameAsync(int requestId, RequestActionDataTransferObject action, CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> CheckAvailabilityAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>> GetBookedDatesAsync(int gameId, int calendarMonth, int calendarYear, CancellationToken cancellationToken = default);
    }
}
