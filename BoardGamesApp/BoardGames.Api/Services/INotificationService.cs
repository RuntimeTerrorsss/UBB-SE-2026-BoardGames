// <copyright file="INotificationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface INotificationService
    {
        ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId);

        NotificationDTO GetNotificationByIdentifier(int notificationId);

        NotificationDTO DeleteNotificationByIdentifier(int notificationId);

        void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto);

        void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto);

        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}
