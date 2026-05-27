// <copyright file="SendNotificationMessage.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace ServerCommunication
{
    public class SendNotificationMessage : MessageBase
    {
        public int UserId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}
