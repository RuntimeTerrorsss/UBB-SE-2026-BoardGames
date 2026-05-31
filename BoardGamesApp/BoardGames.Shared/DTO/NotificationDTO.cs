// <copyright file="NotificationDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class NotificationDTO
    {
        private const string TimeDisplayFormat = "hh:mm tt";

        public int Id { get; set; }

        public UserDTO Recipient { get; set; }

        public DateTime Timestamp { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Informational;

        public int? RelatedRequestId { get; set; }

        public string TimeDisplay => this.Timestamp.ToString(TimeDisplayFormat);

        public NotificationDTO()
        {
        }
    }
}
