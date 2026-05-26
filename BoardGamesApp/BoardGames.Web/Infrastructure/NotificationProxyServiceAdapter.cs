using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;

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
