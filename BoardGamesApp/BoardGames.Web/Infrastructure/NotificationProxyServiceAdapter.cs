// <copyright file="NotificationProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class NotificationProxyServiceAdapter : INotificationProxyService
    {
        private readonly HttpClient httpClient;

        public NotificationProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IReadOnlyList<NotificationDTO>> GetNotificationsForUserAsync(Guid accountId)
        {
            using var response = await this.httpClient.GetAsync($"notifications/user/{accountId}");
            return await HttpProxyClient.ReadAsync<List<NotificationDTO>>(response);
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            using var response = await this.httpClient.DeleteAsync($"notifications/{notificationId}");
            await HttpProxyClient.EnsureSuccessAsync(response);
        }
    }
}
