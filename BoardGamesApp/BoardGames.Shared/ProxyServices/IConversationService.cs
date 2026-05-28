// <copyright file="IConversationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IConversationService
    {
        Task<ServiceResult<IReadOnlyList<ConversationDTO>>> GetConversationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ConversationDTO>> GetConversationByIdAsync(
            int conversationId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<int>> FindOrCreateConversationAsync(
            Guid senderAccountId,
            Guid receiverAccountId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<MessageDataTransferObject>> SendMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<MessageDataTransferObject>> UpdateMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default);
    }
}
