// <copyright file="ConversationProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class ConversationProxyServiceAdapter : IConversationProxyService
    {
        private readonly IConversationService conversationService;

        public ConversationProxyServiceAdapter(IConversationService conversationService)
        {
            this.conversationService = conversationService;
        }

        public async Task<IReadOnlyList<ConversationDTO>> GetConversationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
            => (await this.conversationService.GetConversationsForUserAsync(accountId, cancellationToken)).ThrowIfFailed();

        public async Task<ConversationDTO?> GetConversationByIdAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
        {
            var result = await this.conversationService.GetConversationByIdAsync(conversationId, cancellationToken);
            if (result.Success)
            {
                return result.Data;
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw ProxyResultExtensions.ToException(result);
        }

        public async Task<int> FindOrCreateConversationAsync(
            Guid senderAccountId,
            Guid receiverAccountId,
            CancellationToken cancellationToken = default)
            => (await this.conversationService.FindOrCreateConversationAsync(senderAccountId, receiverAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<MessageDataTransferObject> SendMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default)
            => (await this.conversationService.SendMessageAsync(message, cancellationToken)).ThrowIfFailed();

        public async Task<MessageDataTransferObject> UpdateMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default)
            => (await this.conversationService.UpdateMessageAsync(message, cancellationToken)).ThrowIfFailed();
    }
}
