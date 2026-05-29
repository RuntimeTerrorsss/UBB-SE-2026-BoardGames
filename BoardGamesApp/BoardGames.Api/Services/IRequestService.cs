// <copyright file="IRequestService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IRequestService
    {
        ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId);

        ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId);

        ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId);

        Task<Result<int, CreateRequestError>> CreateRequest(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate);

        Task<Result<int, ApproveRequestError>> ApproveRequest(int requestId, Guid ownerAccountId);

        Task<Result<int, DenyRequestError>> DenyRequest(int requestId, Guid ownerAccountId, string declineReason);

        Result<int, CancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId);

        void OnGameDeactivated(int gameId);

        bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate);

        ImmutableList<BookedDateRange> GetBookedDates(int gameId, int calendarMonth, int calendarYear);

        Task<Result<int, OfferError>> OfferGame(int requestId, Guid offeringOwnerAccountId);
    }

    public sealed class BookedDateRange
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
