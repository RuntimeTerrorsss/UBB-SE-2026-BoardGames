// <copyright file="ConversationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public sealed class ConversationService : ApiServiceBase, IConversationService
    {
        public ConversationService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<IReadOnlyList<ConversationDTO>>> GetConversationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            var client = this.CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<ConversationDTO>>(
                token => client.GetAsync($"api/conversation/user/{accountId}", token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<ConversationDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<ConversationDTO>>.Ok(parsed.Data ?? new List<ConversationDTO>())
                        : ServiceResult<IReadOnlyList<ConversationDTO>>.Fail(parsed);
                },
                cancellationToken);
        }

        public Task<ServiceResult<ConversationDTO>> GetConversationByIdAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
        {
            var client = this.CreateClient();
            return ApiResponseReader.SendAsync<ConversationDTO>(
                token => client.GetAsync($"api/conversation/{conversationId}", token),
                (response, token) => ApiResponseReader.ReadJsonAsync<ConversationDTO>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<int>> FindOrCreateConversationAsync(
            Guid senderAccountId,
            Guid receiverAccountId,
            CancellationToken cancellationToken = default)
        {
            var client = this.CreateClient();
            var body = new { SenderAccountId = senderAccountId, ReceiverAccountId = receiverAccountId };
            return ApiResponseReader.SendAsync<int>(
                token => client.PostAsJsonAsync("api/conversation", body, token),
                async (response, token) =>
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return await ApiResponseReader.ToFailAsync<int>(response, token);
                    }

                    var conversation = await ApiResponseReader.ReadJsonAsync<ConversationDTO>(response, token);
                    return conversation.Success && conversation.Data is not null
                        ? ServiceResult<int>.Ok(conversation.Data.Id)
                        : ServiceResult<int>.Ok(0);
                },
                cancellationToken);
        }

        public Task<ServiceResult<MessageDataTransferObject>> SendMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default)
        {
            var client = this.CreateClient();
            return ApiResponseReader.SendAsync<MessageDataTransferObject>(
                token => client.PostAsJsonAsync("api/conversation/messages", message, token),
                (response, token) => ApiResponseReader.ReadJsonAsync<MessageDataTransferObject>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<MessageDataTransferObject>> UpdateMessageAsync(
            MessageDataTransferObject message,
            CancellationToken cancellationToken = default)
        {
            var client = this.CreateClient();
            return ApiResponseReader.SendAsync<MessageDataTransferObject>(
                token => client.PutAsJsonAsync("api/conversation/messages", message, token),
                (response, token) => ApiResponseReader.ReadJsonAsync<MessageDataTransferObject>(response, token),
                cancellationToken);
        }
    }
}
