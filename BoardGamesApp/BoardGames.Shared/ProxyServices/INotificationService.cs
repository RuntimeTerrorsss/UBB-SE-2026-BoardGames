using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface INotificationService
    {
        Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(int notificationId, CancellationToken cancellationToken = default);

        Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(int notificationId, CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateNotificationByIdentifierAsync(int notificationId, NotificationDTO notification, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(int relatedRequestId, CancellationToken cancellationToken = default);

        Task<ServiceResult> PersistNotificationAsync(NotificationDTO notification, CancellationToken cancellationToken = default);
    }
}
