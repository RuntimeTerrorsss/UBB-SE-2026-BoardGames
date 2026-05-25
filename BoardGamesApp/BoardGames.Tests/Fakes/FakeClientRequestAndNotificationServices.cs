using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeClientRequestService : IRequestService
    {
        public ImmutableList<RequestDTO> RequestsForRenter { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<RequestDTO> RequestsForOwner { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<RequestDTO> OpenRequestsForOwner { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<(DateTime StartDate, DateTime EndDate)> BookedDates { get; set; } =
            ImmutableList<(DateTime StartDate, DateTime EndDate)>.Empty;
        public Result<int, CreateRequestError> CreateRequestResult { get; set; } =
            Result<int, CreateRequestError>.Success(1);
        public Result<int, ApproveRequestError> ApproveRequestResult { get; set; } =
            Result<int, ApproveRequestError>.Success(1);
        public Result<int, DenyRequestError> DenyRequestResult { get; set; } =
            Result<int, DenyRequestError>.Success(1);
        public Result<int, CancelRequestError> CancelRequestResult { get; set; } =
            Result<int, CancelRequestError>.Success(1);
        public Result<int, OfferError> OfferGameResult { get; set; } =
            Result<int, OfferError>.Success(1);
        public bool AvailabilityResult { get; set; } = true;
        public int CreateRequestCallCount { get; private set; }
        public int CancelRequestCallCount { get; private set; }
        public int ApproveRequestCallCount { get; private set; }
        public int DenyRequestCallCount { get; private set; }
        public int LastRequestId { get; private set; }
        public int LastGameId { get; private set; }
        public Guid LastRenterAccountId { get; private set; }
        public Guid LastOwnerAccountId { get; private set; }

        public Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForRenterAsync(
            Guid renterAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<RequestDTO>>.Ok(RequestsForRenter));

        public Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForOwnerAsync(
            Guid ownerAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<RequestDTO>>.Ok(RequestsForOwner));

        public Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetOpenRequestsForOwnerAsync(
            Guid ownerAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<RequestDTO>>.Ok(OpenRequestsForOwner));

        public Task<ServiceResult<int>> CreateRequestAsync(
            CreateRequestDataTransferObject request,
            CancellationToken cancellationToken = default)
        {
            CreateRequestCallCount++;
            LastGameId = request.GameId;
            LastRenterAccountId = request.RenterAccountId;
            LastOwnerAccountId = request.OwnerAccountId;
            return Task.FromResult(CreateRequestResult.IsSuccess
                ? ServiceResult<int>.Ok(CreateRequestResult.Value)
                : MapCreateFailure(CreateRequestResult.Error));
        }

        public Task<ServiceResult<int>> ApproveRequestAsync(
            int requestId,
            Guid ownerAccountId,
            CancellationToken cancellationToken = default)
        {
            ApproveRequestCallCount++;
            LastRequestId = requestId;
            LastOwnerAccountId = ownerAccountId;
            return Task.FromResult(ApproveRequestResult.IsSuccess
                ? ServiceResult<int>.Ok(ApproveRequestResult.Value)
                : MapApproveFailure(ApproveRequestResult.Error));
        }

        public Task<ServiceResult<int>> DenyRequestAsync(
            int requestId,
            RequestActionDataTransferObject action,
            CancellationToken cancellationToken = default)
        {
            DenyRequestCallCount++;
            LastRequestId = requestId;
            LastOwnerAccountId = action.AccountId;
            return Task.FromResult(DenyRequestResult.IsSuccess
                ? ServiceResult<int>.Ok(DenyRequestResult.Value)
                : MapDenyFailure(DenyRequestResult.Error));
        }

        public Task<ServiceResult<int>> CancelRequestAsync(
            int requestId,
            RequestActionDataTransferObject action,
            CancellationToken cancellationToken = default)
        {
            CancelRequestCallCount++;
            LastRequestId = requestId;
            return Task.FromResult(CancelRequestResult.IsSuccess
                ? ServiceResult<int>.Ok(CancelRequestResult.Value)
                : MapCancelFailure(CancelRequestResult.Error));
        }

        public Task<ServiceResult<int>> OfferGameAsync(
            int requestId,
            RequestActionDataTransferObject action,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OfferGameResult.IsSuccess
                ? ServiceResult<int>.Ok(OfferGameResult.Value)
                : MapOfferFailure(OfferGameResult.Error));

        public Task<ServiceResult<bool>> CheckAvailabilityAsync(
            int gameId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<bool>.Ok(AvailabilityResult));

        public Task<ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>> GetBookedDatesAsync(
            int gameId,
            int calendarMonth,
            int calendarYear,
            CancellationToken cancellationToken = default)
        {
            var bookedDateRanges = new List<BookedDateRangeDataTransferObject>();
            foreach (var bookedDate in BookedDates)
            {
                bookedDateRanges.Add(new BookedDateRangeDataTransferObject
                {
                    StartDate = bookedDate.StartDate,
                    EndDate = bookedDate.EndDate,
                });
            }

            return Task.FromResult(
                ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>.Ok(bookedDateRanges));
        }

        private static ServiceResult<int> MapCreateFailure(CreateRequestError error) =>
            error switch
            {
                CreateRequestError.OwnerCannotRent => ServiceResult<int>.Fail(
                    "Owner cannot rent their own game.",
                    HttpStatusCode.BadRequest,
                    "owner_cannot_rent"),
                CreateRequestError.DatesUnavailable => ServiceResult<int>.Fail(
                    "The selected dates are unavailable.",
                    HttpStatusCode.Conflict,
                    "dates_unavailable"),
                CreateRequestError.GameDoesNotExist => ServiceResult<int>.Fail(
                    "Game not found.",
                    HttpStatusCode.NotFound,
                    "game_not_found"),
                _ => ServiceResult<int>.Fail(
                    "The provided date range is invalid.",
                    HttpStatusCode.BadRequest,
                    "invalid_date_range"),
            };

        private static ServiceResult<int> MapApproveFailure(ApproveRequestError error) =>
            error switch
            {
                ApproveRequestError.NotFound => ServiceResult<int>.Fail(
                    "Request not found.",
                    HttpStatusCode.NotFound,
                    "request_not_found"),
                ApproveRequestError.Unauthorized => ServiceResult<int>.Fail(
                    "You are not allowed to approve this request.",
                    HttpStatusCode.Forbidden,
                    "request_forbidden"),
                _ => ServiceResult<int>.Fail(
                    "The request could not be approved.",
                    HttpStatusCode.Conflict,
                    "request_transaction_failed"),
            };

        private static ServiceResult<int> MapDenyFailure(DenyRequestError error) =>
            error == DenyRequestError.Unauthorized
                ? ServiceResult<int>.Fail(
                    "You are not allowed to deny this request.",
                    HttpStatusCode.Forbidden,
                    "request_forbidden")
                : ServiceResult<int>.Fail(
                    "Request not found.",
                    HttpStatusCode.NotFound,
                    "request_not_found");

        private static ServiceResult<int> MapCancelFailure(CancelRequestError error) =>
            error == CancelRequestError.Unauthorized
                ? ServiceResult<int>.Fail(
                    "You are not allowed to cancel this request.",
                    HttpStatusCode.Forbidden,
                    "request_forbidden")
                : ServiceResult<int>.Fail(
                    "Request not found.",
                    HttpStatusCode.NotFound,
                    "request_not_found");

        private static ServiceResult<int> MapOfferFailure(OfferError error) =>
            error switch
            {
                OfferError.NotFound => ServiceResult<int>.Fail(
                    "Request not found.",
                    HttpStatusCode.NotFound,
                    "request_not_found"),
                OfferError.NotOwner => ServiceResult<int>.Fail(
                    "You are not allowed to offer for this request.",
                    HttpStatusCode.Forbidden,
                    "request_forbidden"),
                OfferError.RequestNotOpen => ServiceResult<int>.Fail(
                    "The request is no longer open.",
                    HttpStatusCode.Conflict,
                    "request_not_open"),
                _ => ServiceResult<int>.Fail(
                    "The offer could not be completed.",
                    HttpStatusCode.Conflict,
                    "request_transaction_failed"),
            };
    }

    internal sealed class FakeClientNotificationService : IDesktopNotificationService
    {
        public ImmutableList<NotificationDTO> NotificationsForUser { get; set; } =
            ImmutableList<NotificationDTO>.Empty;
        public int SendNotificationCallCount { get; private set; }
        public int DeleteNotificationCallCount { get; private set; }
        public int DeleteLinkedNotificationCallCount { get; private set; }
        public Guid LastRecipientAccountId { get; private set; }
        public int LastDeletedNotificationId { get; private set; }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer) => new EmptyDisposable();

        public Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<NotificationDTO>.Ok(new NotificationDTO { Id = notificationId }));

        public Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default)
        {
            DeleteNotificationCallCount++;
            LastDeletedNotificationId = notificationId;
            return Task.FromResult(ServiceResult<NotificationDTO>.Ok(new NotificationDTO { Id = notificationId }));
        }

        public Task<ServiceResult> UpdateNotificationByIdentifierAsync(
            int notificationId,
            NotificationDTO updatedNotification,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult.Ok());

        public Task<ServiceResult> SendNotificationToUserAsync(
            Guid recipientAccountId,
            NotificationDTO notification,
            CancellationToken cancellationToken = default)
        {
            SendNotificationCallCount++;
            LastRecipientAccountId = recipientAccountId;
            return Task.FromResult(ServiceResult.Ok());
        }

        public Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<NotificationDTO>>.Ok(NotificationsForUser));

        public Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(
            int relatedRequestId,
            CancellationToken cancellationToken = default)
        {
            DeleteLinkedNotificationCallCount++;
            return Task.FromResult(ServiceResult.Ok());
        }

        public void SubscribeToServer(Guid accountId)
        {
        }

        public void StartListening()
        {
        }

        public void StopListening()
        {
        }
    }

    internal sealed class FakeServerClient : IServerClient
    {
        public event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public NotificationConnectionStatus ConnectionStatus { get; set; }
        public int SubscribeToServerCallCount { get; private set; }
        public int StopListeningCallCount { get; private set; }
        public int LastTargetUserId { get; private set; }

        public IDisposable Subscribe(IObserver<IncomingNotification> observer) => new EmptyDisposable();

        public Task ListenAsync() => Task.CompletedTask;

        public void SubscribeToServer(int targetUserId)
        {
            SubscribeToServerCallCount++;
            LastTargetUserId = targetUserId;
        }

        public void SendNotification(int targetUserId, string notificationTitle, string notificationBody)
        {
        }

        public void StopListening()
        {
            StopListeningCallCount++;
        }

        public void RaiseConnectionStatusChanged(NotificationConnectionStatus status)
        {
            ConnectionStatus = status;
            ConnectionStatusChanged?.Invoke(
                this,
                new NotificationConnectionStatusChangedEventArgs(status));
        }
    }

    internal sealed class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
