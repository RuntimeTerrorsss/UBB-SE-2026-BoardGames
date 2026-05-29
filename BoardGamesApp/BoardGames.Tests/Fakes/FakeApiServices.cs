// <copyright file="FakeApiServices.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using ApiRequestService = BoardGames.Api.Services.IRequestService;
using ApiNotificationService = BoardGames.Api.Services.INotificationService;
using ApiResultCreate = BoardGames.Api.Services.Result<int, BoardGames.Api.Services.CreateRequestError>;
using ApiResultApprove = BoardGames.Api.Services.Result<int, BoardGames.Api.Services.ApproveRequestError>;
using ApiResultDeny = BoardGames.Api.Services.Result<int, BoardGames.Api.Services.DenyRequestError>;
using ApiResultCancel = BoardGames.Api.Services.Result<int, BoardGames.Api.Services.CancelRequestError>;
using ApiResultOffer = BoardGames.Api.Services.Result<int, BoardGames.Api.Services.OfferError>;
using ApiConversationService = BoardGames.Api.Services.IConversationApiService;
using BookedDateRange = BoardGames.Api.Services.BookedDateRange;
using CreateRequestError = BoardGames.Api.Services.CreateRequestError;
using ApproveRequestError = BoardGames.Api.Services.ApproveRequestError;
using DenyRequestError = BoardGames.Api.Services.DenyRequestError;
using CancelRequestError = BoardGames.Api.Services.CancelRequestError;
using OfferError = BoardGames.Api.Services.OfferError;
using BoardGames.Shared.DTO;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeApiRequestService : ApiRequestService
    {
        public int OnGameDeactivatedCallCount { get; private set; }

        public int LastDeactivatedGameId { get; private set; }

        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) =>
            ImmutableList<RequestDTO>.Empty;

        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) =>
            ImmutableList<RequestDTO>.Empty;

        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) =>
            ImmutableList<RequestDTO>.Empty;

        public Task<ApiResultCreate> CreateRequest(
            int gameId,
            Guid renterAccountId,
            Guid ownerAccountId,
            DateTime startDate,
            DateTime endDate) => Task.FromResult(ApiResultCreate.Failure(CreateRequestError.GameDoesNotExist));

        public Task<ApiResultApprove> ApproveRequest(int requestId, Guid ownerAccountId) =>
            Task.FromResult(ApiResultApprove.Failure(ApproveRequestError.NotFound));

        public Task<ApiResultDeny> DenyRequest(int requestId, Guid ownerAccountId, string declineReason) =>
            Task.FromResult(ApiResultDeny.Failure(DenyRequestError.NotFound));

        public Task<ApiResultCancel> CancelRequest(int requestId, Guid cancellingAccountId) =>
            Task.FromResult(ApiResultCancel.Failure(CancelRequestError.NotFound));

        public void OnGameDeactivated(int gameId)
        {
            this.OnGameDeactivatedCallCount++;
            this.LastDeactivatedGameId = gameId;
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate) => true;

        public ImmutableList<BookedDateRange> GetBookedDates(int gameId, int calendarMonth, int calendarYear) =>
            ImmutableList<BookedDateRange>.Empty;

        public Task<ApiResultOffer> OfferGame(int requestId, Guid offeringOwnerAccountId) =>
            Task.FromResult(ApiResultOffer.Failure(OfferError.NotFound));
    }

    internal sealed class FakeConversationApiService : ApiConversationService
    {
        public int AttachRentalRequestMessageCallCount { get; private set; }

        public int FinalizeRentalRequestMessageCallCount { get; private set; }

        public bool LastFinalizeAccepted { get; private set; }

        public Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId) =>
            Task.FromResult(new List<ConversationDTO>());

        public Task<ConversationDTO?> GetConversationById(int conversationId) =>
            Task.FromResult<ConversationDTO?>(null);

        public Task<MessageDataTransferObject> SendMessage(MessageDataTransferObject dto) =>
            Task.FromResult(dto);

        public Task<MessageDataTransferObject?> UpdateMessage(MessageDataTransferObject dto) =>
            Task.FromResult<MessageDataTransferObject?>(dto);

        public Task HandleReadReceipt(ReadReceiptDTO dto) => Task.CompletedTask;

        public Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB) =>
            Task.FromResult(1);

        public Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end)
        {
            this.AttachRentalRequestMessageCallCount++;
            return Task.CompletedTask;
        }

        public Task AcceptRentalRequestMessage(int requestId, int rentalId) => Task.CompletedTask;

        public Task FinalizeRentalRequestMessage(int requestId, bool accepted)
        {
            this.FinalizeRentalRequestMessageCallCount++;
            this.LastFinalizeAccepted = accepted;
            return Task.CompletedTask;
        }

        public Task<MessageDataTransferObject?> CreateCashAgreementMessage(int parentMessageId, int paymentId) =>
            Task.FromResult<MessageDataTransferObject?>(null);
    }

    internal sealed class FakeApiNotificationService : ApiNotificationService
    {
        public int DeleteLinkedNotificationCallCount { get; private set; }

        public int SendNotificationCallCount { get; private set; }

        public int LastLinkedRequestId { get; private set; }

        public Guid LastRecipientAccountId { get; private set; }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId) =>
            ImmutableList<NotificationDTO>.Empty;

        public NotificationDTO GetNotificationByIdentifier(int notificationId) => new NotificationDTO { Id = notificationId };

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) => new NotificationDTO { Id = notificationId };

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto)
        {
        }

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto)
        {
            this.SendNotificationCallCount++;
            this.LastRecipientAccountId = recipientAccountId;
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            this.DeleteLinkedNotificationCallCount++;
            this.LastLinkedRequestId = relatedRequestId;
        }
    }
}
