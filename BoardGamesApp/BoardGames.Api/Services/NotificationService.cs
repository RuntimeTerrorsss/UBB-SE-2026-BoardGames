// <copyright file="NotificationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Api.Mappers;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

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
            this.notificationRepository.GetNotificationsByUser(accountId)
                .Select(model => this.notificationMapper.ToDTO(model)!)
                .ToImmutableList();

        public NotificationDTO GetNotificationByIdentifier(int notificationId) =>
            this.notificationMapper.ToDTO(this.notificationRepository.Get(notificationId))!;

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) =>
            this.notificationMapper.ToDTO(this.notificationRepository.Delete(notificationId))!;

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedDto) =>
            this.notificationRepository.Update(notificationId, this.notificationMapper.ToModel(updatedDto)!);

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
                Type = NotificationMapper.ToModelType(notificationToSend.Type),
                RelatedRequest = notificationToSend.RelatedRequestId.HasValue ? new Request { Id = notificationToSend.RelatedRequestId.Value } : null,
            };

            this.notificationRepository.Add(model);
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId) =>
            this.notificationRepository.DeleteNotificationsLinkedToRequest(linkedRequestId);
    }
}
