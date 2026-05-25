// <copyright file="Notification.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Enums;

namespace BoardGames.Data.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public User? Recipient { get; set; }

        public DateTime Timestamp { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Informational;

        public Request? RelatedRequest { get; set; }

        public Notification()
        {
        }

        public Notification(int id, User? recipientAccount, DateTime timestamp, string title, string body,
                            NotificationType notificationType = NotificationType.Informational, Request? relatedRequest = null)
        {
            this.Id = id;
            this.Recipient = recipientAccount;
            this.Timestamp = timestamp;
            this.Title = title;
            this.Body = body;
            this.Type = notificationType;
            this.RelatedRequest = relatedRequest;
        }
    }
}
