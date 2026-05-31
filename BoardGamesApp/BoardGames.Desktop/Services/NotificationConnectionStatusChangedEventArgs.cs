using System;

namespace BoardGames.Desktop.Services
{
    public sealed class NotificationConnectionStatusChangedEventArgs : EventArgs
    {
        public NotificationConnectionStatusChangedEventArgs(NotificationConnectionStatus connectionStatus)
        {
            ConnectionStatus = connectionStatus;
        }

        public NotificationConnectionStatus ConnectionStatus { get; }
    }
}
