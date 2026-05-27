// <copyright file="NotificationProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class NotificationProxyServiceAdapter : INotificationProxyService
    {
        private readonly INotificationService notificationService;

        public NotificationProxyServiceAdapter(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public async Task<IReadOnlyList<NotificationDTO>> GetNotificationsForUserAsync(Guid accountId)
            => (await this.notificationService.GetNotificationsForUserAsync(accountId)).ThrowIfFailed();

        public async Task DeleteNotificationAsync(int notificationId)
            => (await this.notificationService.DeleteNotificationByIdentifierAsync(notificationId)).ThrowIfFailed();
    }
}
