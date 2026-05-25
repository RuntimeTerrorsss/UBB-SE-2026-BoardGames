using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Web.Infrastructure;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using BoardGames.Web.Infrastructure;

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
            => (await notificationService.GetNotificationsForUserAsync(accountId)).ThrowIfFailed();

        public async Task DeleteNotificationAsync(int notificationId)
            => (await notificationService.DeleteNotificationByIdentifierAsync(notificationId)).ThrowIfFailed();
    }
}
