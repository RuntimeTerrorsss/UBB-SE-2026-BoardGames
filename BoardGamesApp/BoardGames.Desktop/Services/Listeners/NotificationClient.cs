// <copyright file="NotificationClient.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Sockets;
using System.Text;
using BoardGames.Shared.DTO;
using ServerCommunication;

namespace BoardGames.Desktop.Services.Listeners
{
    public class NotificationClient : IServerClient, IDisposable
    {
        private const int NotificationServerPort = 4544;
        private const int AutoAssignLocalUdpPort = 0;
        private const int InitialRetryCount = 0;
        private const int RetryBackoffMultiplier = 2;
        private bool isDisposed;

        private readonly List<IObserver<IncomingNotification>> incomingNotificationSubscribers = new();
        private readonly UdpClient udpSocketClient;

        private readonly CancellationTokenSource listenCancellationSource = new();

        private CancellationToken ListenCancellationToken => this.listenCancellationSource.Token;

        private const int MaxRetries = 5;
        private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

        public IPEndPoint ServerEndpoint => new IPEndPoint(IPAddress.Loopback, NotificationServerPort);

        private NotificationConnectionStatus connectionStatus = NotificationConnectionStatus.Stopped;

        public NotificationConnectionStatus ConnectionStatus => this.connectionStatus;

        public event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public NotificationClient()
        {
            this.udpSocketClient = new UdpClient(AutoAssignLocalUdpPort);
        }

        private void HandleMessagePacket(MessageWrapper wrappedMessage)
        {
            try
            {
                switch (wrappedMessage.Type)
                {
                    case nameof(SendNotificationMessage):
                        this.HandleSendNotificationMessage(wrappedMessage);
                        break;
                    default:
                        Console.WriteLine($"Message type cannot be handled: {wrappedMessage.Type}");
                        break;
                }
            }
            catch (Exception messageHandlingException)
            {
                Console.WriteLine($"Exception when handling message packet: {messageHandlingException.Message}");
            }
        }

        private void HandleSendNotificationMessage(MessageWrapper wrappedMessage)
        {
            SendNotificationMessage? deserializedMessage = wrappedMessage.Deserialize<SendNotificationMessage>();

            if (deserializedMessage == null)
            {
                throw new ArgumentNullException(nameof(deserializedMessage));
            }

            var incomingNotification = new IncomingNotification
            {
                UserId = deserializedMessage.UserId,
                Timestamp = deserializedMessage.Timestamp,
                Title = deserializedMessage.Title,
                Body = deserializedMessage.Body,
            };

            foreach (var subscriber in this.incomingNotificationSubscribers)
            {
                subscriber.OnNext(incomingNotification);
            }
        }

        public void StopListening() => this.listenCancellationSource.Cancel();

        public async Task ListenAsync()
        {
            int currentRetryCount = InitialRetryCount;
            var currentRetryDelay = InitialRetryDelay;
            this.UpdateConnectionStatus(NotificationConnectionStatus.Reconnecting);

            while (!this.ListenCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var receivedResult = await this.udpSocketClient.ReceiveAsync(this.ListenCancellationToken);
                    this.UpdateConnectionStatus(NotificationConnectionStatus.Connected);
                    currentRetryCount = InitialRetryCount;
                    currentRetryDelay = InitialRetryDelay;

                    MessageWrapper? wrappedMessage = CommunicationHelper.GetMessageWrapper(receivedResult.Buffer);

                    if (wrappedMessage == null)
                    {
                        Console.WriteLine($"Received bad json: {Encoding.UTF8.GetString(receivedResult.Buffer)}");
                        continue;
                    }

                    this.HandleMessagePacket(wrappedMessage);
                }
                catch (SocketException socketException)
                {
                    currentRetryCount++;
                    this.UpdateConnectionStatus(NotificationConnectionStatus.Reconnecting);

                    if (currentRetryCount > MaxRetries)
                    {
                        Console.WriteLine($"UDP client: max retries ({MaxRetries}) reached, stopping. Last error: {socketException.Message}");
                        this.UpdateConnectionStatus(NotificationConnectionStatus.Offline);
                        break;
                    }

                    Console.WriteLine($"UDP client: SocketException ({socketException.Message}), retry {currentRetryCount}/{MaxRetries} in {currentRetryDelay.TotalSeconds}s");
                    try
                    {
                        await Task.Delay(currentRetryDelay, this.ListenCancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    currentRetryDelay = TimeSpan.FromTicks(Math.Min(currentRetryDelay.Ticks * RetryBackoffMultiplier, MaxRetryDelay.Ticks));
                }
                catch (OperationCanceledException)
                {
                    this.UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
                    break;
                }
                catch (ObjectDisposedException)
                {
                    this.UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
                    break;
                }
            }

            if (this.ConnectionStatus != NotificationConnectionStatus.Offline)
            {
                this.UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
            }
        }

        public IDisposable Subscribe(IObserver<IncomingNotification> newObserver)
        {
            this.incomingNotificationSubscribers.Add(newObserver);
            return new Unsubscriber(this.incomingNotificationSubscribers, newObserver);
        }

        public void SendNotification(int recipientUserId, string notificationTitle, string notificationBody)
        {
            var outgoingNotificationMessage = new SendNotificationMessage
            {
                UserId = recipientUserId,
                Timestamp = DateTime.UtcNow,
                Title = notificationTitle,
                Body = notificationBody,
            };

            byte[] serializedData = CommunicationHelper.SerializeMessage(outgoingNotificationMessage);
            this.udpSocketClient.Send(serializedData, serializedData.Length, this.ServerEndpoint);
        }

        public void SubscribeToServer(int subscribingUserId)
        {
            var subscriptionMessage = new SubscribeToServerMessage { UserId = subscribingUserId };
            byte[] serializedData = CommunicationHelper.SerializeMessage(subscriptionMessage);
            this.udpSocketClient.Send(serializedData, serializedData.Length, this.ServerEndpoint);
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.listenCancellationSource.Cancel();
            this.udpSocketClient.Close();
            this.listenCancellationSource.Dispose();

            this.UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
        }

        private void UpdateConnectionStatus(NotificationConnectionStatus newStatus)
        {
            if (this.connectionStatus == newStatus)
            {
                return;
            }

            this.connectionStatus = newStatus;
            this.ConnectionStatusChanged?.Invoke(this, new NotificationConnectionStatusChangedEventArgs(newStatus));
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<IncomingNotification>> subscribersList;
            private readonly IObserver<IncomingNotification> subscriberToRemove;

            public Unsubscriber(List<IObserver<IncomingNotification>> subscribers, IObserver<IncomingNotification> observer)
            {
                this.subscribersList = subscribers;
                this.subscriberToRemove = observer;
            }

            public void Dispose() => this.subscribersList.Remove(this.subscriberToRemove);
        }
    }
}
