// <copyright file="RequestService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Mappers;
using BoardGames.Data.Constants;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using DataRequestStatus = BoardGames.Data.Enums.RequestStatus;
using DtoRequestStatus = BoardGames.Shared.DTO.RequestStatus;

namespace BoardGames.Api.Services
{
    public class RequestService : IRequestService
    {
        private const int NewRequestId = 0;
        private const int MissingForeignKeyId = 0;
        private const int MissingOptionalDatePart = 0;
        private const int AvailabilityWindowMonths = 1;

        private readonly IRequestRepository requestDataRepository;
        private readonly IRentalRepository rentalConflictRepository;
        private readonly INotificationService requestNotificationService;
        private readonly IGameRepository gameValidationRepository;
        private readonly IConversationApiService conversationApiService;
        private readonly RequestMapper requestDtoMapper;

        public RequestService(
            IRequestRepository requestRepository,
            IRentalRepository rentalRepository,
            IGameRepository gameRepository,
            INotificationService notificationService,
            IConversationApiService conversationApiService,
            RequestMapper requestMapper)
        {
            this.requestDataRepository = requestRepository;
            this.rentalConflictRepository = rentalRepository;
            this.gameValidationRepository = gameRepository;
            this.requestNotificationService = notificationService;
            this.conversationApiService = conversationApiService;
            this.requestDtoMapper = requestMapper;
        }

        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) =>
            this.requestDataRepository.GetRequestsByRenter(renterAccountId).Select(request => this.requestDtoMapper.ToDTO(request)!).ToImmutableList();

        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) =>
            this.requestDataRepository.GetRequestsByOwner(ownerAccountId).Select(request => this.requestDtoMapper.ToDTO(request)!).ToImmutableList();

        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) =>
            this.GetRequestsForOwner(ownerAccountId).Where(request => request.Status == DtoRequestStatus.Open).ToImmutableList();

        public async Task<Result<int, CreateRequestError>> CreateRequest(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            if (!DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate))
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.InvalidDateRange);
            }

            Game game;
            try
            {
                game = this.gameValidationRepository.GetGame(gameId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist);
            }

            var effectiveOwnerId = game.Owner?.Id ?? ownerAccountId;
            if (renterAccountId == effectiveOwnerId)
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent);
            }

            if (!this.CheckAvailability(gameId, startDate, endDate))
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable);
            }

            var newRequest = new Request(NewRequestId, new Game { Id = gameId }, new Account { Id = renterAccountId }, new Account { Id = effectiveOwnerId }, startDate, endDate);
            this.requestDataRepository.Add(newRequest);

            var ownerNotificationGameName = game.Name ?? "a game";
            this.SendNotificationToAccount(effectiveOwnerId, NotificationTitles.RentalRequestReceived,
                $"You have a new rental request for {ownerNotificationGameName} {FormatPeriod(startDate, endDate)}.",
                relatedRequestId: newRequest.Id);

            try
            {
                await this.conversationApiService.AttachRentalRequestMessage(newRequest.Id, renterAccountId, effectiveOwnerId, ownerNotificationGameName, startDate, endDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] AttachRentalRequestMessage failed for request {newRequest.Id}: {ex.Message}");
            }

            return Result<int, CreateRequestError>.Success(newRequest.Id);
        }

        public async Task<Result<int, ApproveRequestError>> ApproveRequest(int requestId, Guid ownerAccountId)
        {
            Request req;
            try
            {
                req = this.requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.NotFound);
            }

            if (req.Owner?.Id != ownerAccountId)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.Unauthorized);
            }

            if (req.Status != DataRequestStatus.Open)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.NotFound);
            }

            var (success, rentalId) = await this.TryApproveOpenRequestAndNotify(req);
            if (!success)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.TransactionFailed);
            }

            return Result<int, ApproveRequestError>.Success(rentalId);
        }

        public async Task<Result<int, DenyRequestError>> DenyRequest(int requestId, Guid ownerAccountId, string denialReason)
        {
            Request req;
            try
            {
                req = this.requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, DenyRequestError>.Failure(DenyRequestError.NotFound);
            }

            if (req.Owner?.Id != ownerAccountId)
            {
                return Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized);
            }

            var reason = string.IsNullOrWhiteSpace(denialReason?.Trim()) ? DialogMessages.NoReasonProvided : denialReason!.Trim();
            this.requestNotificationService.DeleteNotificationsLinkedToRequest(requestId);
            this.requestDataRepository.Delete(requestId);

            var renterId = req.Renter?.Id ?? Guid.Empty;
            var gameName = req.Game?.Name ?? "the selected game";
            this.SendNotificationToAccount(renterId, NotificationTitles.RentalRequestDeclined,
                $"Your request for {gameName} {FormatPeriod(req.StartDate, req.EndDate)} was declined. Reason: {reason}");

            try
            {
                await this.conversationApiService.FinalizeRentalRequestMessage(requestId, accepted: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] FinalizeRentalRequestMessage(deny) failed for request {requestId}: {ex.Message}");
            }

            return Result<int, DenyRequestError>.Success(requestId);
        }

        public Result<int, CancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId)
        {
            Request req;
            try
            {
                req = this.requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound);
            }

            if (req.Renter?.Id != cancellingAccountId)
            {
                return Result<int, CancelRequestError>.Failure(CancelRequestError.Unauthorized);
            }

            this.requestNotificationService.DeleteNotificationsLinkedToRequest(requestId);
            try
            {
                this.requestDataRepository.Delete(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound);
            }

            return Result<int, CancelRequestError>.Success(requestId);
        }

        public void OnGameDeactivated(int deactivatedGameId)
        {
            var pending = this.requestDataRepository.GetRequestsByGame(deactivatedGameId)
                .Where(request => request.Status == DataRequestStatus.Open || request.Status == DataRequestStatus.OfferPending).ToImmutableList();

            foreach (var pendingRequest in pending)
            {
                this.requestNotificationService.DeleteNotificationsLinkedToRequest(pendingRequest.Id);
                this.requestDataRepository.Delete(pendingRequest.Id);

                var renterId = pendingRequest.Renter?.Id ?? Guid.Empty;
                var gameName = pendingRequest.Game?.Name ?? "the selected game";
                this.SendNotificationToAccount(renterId, NotificationTitles.RentalRequestCancelled,
                    $"Your request for {gameName} {FormatPeriod(pendingRequest.StartDate, pendingRequest.EndDate)} has been cancelled because the game is no longer available.");
            }
        }

        public ImmutableList<BookedDateRange> GetBookedDates(int gameId, int month = MissingOptionalDatePart, int year = MissingOptionalDatePart)
        {
            if (month == MissingOptionalDatePart)
            {
                month = DateTime.UtcNow.Month;
            }

            if (year == MissingOptionalDatePart)
            {
                year = DateTime.UtcNow.Year;
            }

            return this.requestDataRepository.GetRequestsByGame(gameId)
                .Where(request => request.StartDate.Month == month && request.StartDate.Year == year)
                .OrderBy(request => request.StartDate)
                .Select(request => new BookedDateRange
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate.AddHours(DomainConstants.RentalBufferHours),
                })
                .ToImmutableList();
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            if (startDate > DateTime.UtcNow.AddMonths(AvailabilityWindowMonths) || endDate > DateTime.UtcNow.AddMonths(AvailabilityWindowMonths))
            {
                return false;
            }

            Game game;
            try
            {
                game = this.gameValidationRepository.GetGame(gameId);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            if (!game.IsActive)
            {
                return false;
            }

            bool rentalConflict = this.rentalConflictRepository.GetRentalsByGame(gameId)
                .Any(rental => startDate < rental.EndDate.AddHours(DomainConstants.RentalBufferHours) && endDate > rental.StartDate.AddHours(-DomainConstants.RentalBufferHours));
            if (rentalConflict)
            {
                return false;
            }

            return !this.requestDataRepository.GetRequestsByGame(gameId)
                .Any(request => request.StartDate.AddHours(-DomainConstants.RentalBufferHours) < endDate && request.EndDate.AddHours(DomainConstants.RentalBufferHours) > startDate);
        }

        public async Task<Result<int, OfferError>> OfferGame(int requestId, Guid offeringOwnerAccountId)
        {
            Request req;
            try
            {
                req = this.requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, OfferError>.Failure(OfferError.NotFound);
            }

            if (req.Owner?.Id != offeringOwnerAccountId)
            {
                return Result<int, OfferError>.Failure(OfferError.NotOwner);
            }

            if (req.Status != DataRequestStatus.Open)
            {
                return Result<int, OfferError>.Failure(OfferError.RequestNotOpen);
            }

            var (success, rentalId) = await this.TryApproveOpenRequestAndNotify(req);
            if (!success)
            {
                return Result<int, OfferError>.Failure(OfferError.TransactionFailed);
            }

            return Result<int, OfferError>.Success(rentalId);
        }

        private void SendNotificationToAccount(Guid accountId, string title, string body, NotificationType type = default, int? relatedRequestId = null)
        {
            if (accountId == Guid.Empty)
            {
                return;
            }

            this.requestNotificationService.SendNotificationToUser(accountId, new NotificationDTO
            {
                Id = NewRequestId,
                Recipient = new UserDTO { Id = accountId },
                Timestamp = DateTime.UtcNow,
                Title = title,
                Body = body,
                Type = type,
                RelatedRequestId = relatedRequestId,
            });
        }

        private async Task<(bool Success, int RentalId)> TryApproveOpenRequestAndNotify(Request req)
        {
            var buffStart = req.StartDate.AddHours(-DomainConstants.RentalBufferHours);
            var buffEnd = req.EndDate.AddHours(DomainConstants.RentalBufferHours);
            var conflicts = this.requestDataRepository.GetOverlappingRequests(req.Game?.Id ?? MissingForeignKeyId, req.Id, buffStart, buffEnd);

            int rentalId;
            try
            {
                rentalId = this.requestDataRepository.ApproveAtomically(req, conflicts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TryApproveOpenRequestAndNotify failed: {ex}");
                return (false, MissingForeignKeyId);
            }

            var gameName = req.Game?.Name ?? "the selected game";
            foreach (var conflict in conflicts)
            {
                var conflictRenterId = conflict.Renter?.Id ?? Guid.Empty;
                this.SendNotificationToAccount(conflictRenterId, NotificationTitles.BookingUnavailable,
                    $"Your request for {gameName} {FormatPeriod(conflict.StartDate, conflict.EndDate)} was declined because the game is no longer available in that period.");
            }

            var renterId = req.Renter?.Id ?? Guid.Empty;
            this.SendNotificationToAccount(renterId, NotificationTitles.RentalRequestApproved,
                $"Your request for {gameName} {FormatPeriod(req.StartDate, req.EndDate)} was approved.");

            try
            {
                await this.conversationApiService.FinalizeRentalRequestMessage(req.Id, accepted: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] FinalizeRentalRequestMessage(approve) failed for request {req.Id}: {ex.Message}");
            }

            return (true, rentalId);
        }

        private static string FormatPeriod(DateTime start, DateTime end) => $"({start:d}-{end:d})";
    }
}
