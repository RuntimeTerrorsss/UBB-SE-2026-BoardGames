using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardGames.Api.Services
{
    public class NotificationService : INotificationService
    {
        private const int NewNotificationId = 0;

        private readonly INotificationRepository notificationRepository;
        private readonly NotificationMapper notificationMapper;

        public NotificationService(INotificationRepository notificationRepository, NotificationMapper notificationMapper)
        {
            this.notificationRepository = notificationRepository;
            this.notificationMapper = notificationMapper;
        }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId) =>
            notificationRepository.GetNotificationsByUser(accountId)
                .Select(model => notificationMapper.ToDTO(model)!)
                .ToImmutableList();

        public NotificationDTO GetNotificationByIdentifier(int notificationId) =>
            notificationMapper.ToDTO(notificationRepository.Get(notificationId))!;

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) =>
            notificationMapper.ToDTO(notificationRepository.Delete(notificationId))!;

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedDto) =>
            notificationRepository.Update(notificationId, notificationMapper.ToModel(updatedDto)!);

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationToSend)
        {
            if (notificationToSend == null)
            {
                throw new ArgumentNullException(nameof(notificationToSend));
            }

            DateTime timestamp = notificationToSend.Timestamp == default ? DateTime.UtcNow : notificationToSend.Timestamp;
            var model = new Notification
            {
                Id = NewNotificationId,
                Recipient = new Account { Id = recipientAccountId },
                Timestamp = timestamp,
                Title = notificationToSend.Title,
                Body = notificationToSend.Body,
                Type = notificationToSend.Type,
                RelatedRequest = notificationToSend.RelatedRequestId.HasValue ? new Request { Id = notificationToSend.RelatedRequestId.Value } : null,
            };

            notificationRepository.Add(model);
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId) =>
            notificationRepository.DeleteNotificationsLinkedToRequest(linkedRequestId);
    }
}
