// <copyright file="IRequestService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IRequestService
    {
        Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> CreateRequestAsync(CreateRequestDTO request, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> ApproveRequestAsync(int requestId, Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> DenyRequestAsync(int requestId, RequestActionDTO action, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> CancelRequestAsync(int requestId, RequestActionDTO action, CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> OfferGameAsync(int requestId, RequestActionDTO action, CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> CheckAvailabilityAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<BookedDateRangeDTO>>> GetBookedDatesAsync(int gameId, int calendarMonth, int calendarYear, CancellationToken cancellationToken = default);
    }
}
