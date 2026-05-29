// <copyright file="NotificationConnectionStatusChangedEventArgs.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Services
{
    public sealed class NotificationConnectionStatusChangedEventArgs : EventArgs
    {
        public NotificationConnectionStatusChangedEventArgs(NotificationConnectionStatus connectionStatus)
        {
            this.ConnectionStatus = connectionStatus;
        }

        public NotificationConnectionStatus ConnectionStatus { get; }
    }
}
