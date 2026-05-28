// <copyright file="ConversationProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net;
using BoardGames.Shared.DTO;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class ConversationProxyServiceAdapter : IConversationProxyService, IChatProxyService
    {
        private readonly HttpClient httpClient;

        public ConversationProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IReadOnlyList<ConversationDTO>> GetConversationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"conversation/user/{accountId}", cancellationToken);
            return await HttpProxyClient.ReadAsync<List<ConversationDTO>>(response, cancellationToken);
        }

        public async Task<ConversationDTO?> GetConversationByIdAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync($"conversation/{conversationId}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await HttpProxyClient.ReadAsync<ConversationDTO>(response, cancellationToken);
        }

        public async Task<int> FindOrCreateConversationAsync(
            Guid senderAccountId,
            Guid receiverAccountId,
            CancellationToken cancellationToken = default)
        {
            var body = new { SenderAccountId = senderAccountId, ReceiverAccountId = receiverAccountId };
            using var response = await this.httpClient.PostAsJsonAsync("conversation", body, cancellationToken);
            var conversation = await HttpProxyClient.ReadAsync<ConversationDTO>(response, cancellationToken);
            return conversation.Id;
        }

        public async Task<MessageDataTransferObject> SendMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("conversation/messages", message, cancellationToken);
            return await HttpProxyClient.ReadAsync<MessageDataTransferObject>(response, cancellationToken);
        }

        public async Task<MessageDataTransferObject> UpdateMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PutAsJsonAsync("conversation/messages", message, cancellationToken);
            return await HttpProxyClient.ReadAsync<MessageDataTransferObject>(response, cancellationToken);
        }
    }
}
