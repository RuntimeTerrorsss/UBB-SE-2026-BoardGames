// <copyright file="IServerClient.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Services
{
    using BoardGames.Shared.DTO;

    public interface IServerClient : IObservable<IncomingNotification>
    {
        Task ListenAsync();

        void SubscribeToServer(int targetUserId);

        void SendNotification(int targetUserId, string notificationTitle, string notificationBody);

        void StopListening();

        NotificationConnectionStatus ConnectionStatus { get; }

        event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
    }
}
