// <copyright file="IConversationApiService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IConversationApiService
    {
        Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId);

        Task<ConversationDTO?> GetConversationById(int conversationId);

        Task<MessageDataTransferObject> SendMessage(MessageDataTransferObject dto);

        Task<MessageDataTransferObject?> UpdateMessage(MessageDataTransferObject dto);

        Task HandleReadReceipt(BoardGames.Data.Models.ReadReceiptDTO dto);

        Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB);

        Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end);

        Task AcceptRentalRequestMessage(int requestId, int rentalId);

        Task FinalizeRentalRequestMessage(int requestId, bool accepted);

        Task<MessageDataTransferObject?> CreateCashAgreementMessage(int parentMessageId, int paymentId);
    }
}
