using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using ApiNotificationService = BoardRentAndProperty.ApiClient.INotificationService;
using CurrentUserContextInterface = BoardRentAndProperty.Utilities.ICurrentUserContext;

namespace BoardGames.Desktop.Services
{
    public sealed class DesktopNotificationService :
        IDesktopNotificationService,
        IObserver<IncomingNotification>,
        IDisposable
    {
        private const int NewNotificationId = 0;

        private readonly ApiNotificationService apiNotificationService;
        private readonly IServerClient serverNotificationClient;
        private readonly CurrentUserContextInterface currentUserContext;
        private readonly IToastNotificationService toastNotificationService;
        private readonly List<IObserver<NotificationDTO>> notificationObservers = new();
        private readonly object notificationObserversLock = new();
        private bool isDisposed;

        public DesktopNotificationService(
            ApiNotificationService apiNotificationService,
            IServerClient serverNotificationClient,
            CurrentUserContextInterface currentUserContext,
            IToastNotificationService toastNotificationService)
        {
            this.apiNotificationService = apiNotificationService;
            this.serverNotificationClient = serverNotificationClient;
            this.currentUserContext = currentUserContext;
            this.toastNotificationService = toastNotificationService;
            this.serverNotificationClient.Subscribe(this);
        }

        public Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default) =>
            apiNotificationService.GetNotificationByIdentifierAsync(notificationId, cancellationToken);

        public Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default) =>
            apiNotificationService.DeleteNotificationByIdentifierAsync(notificationId, cancellationToken);

        public Task<ServiceResult> UpdateNotificationByIdentifierAsync(
            int notificationId,
            NotificationDTO updatedNotification,
            CancellationToken cancellationToken = default) =>
            apiNotificationService.UpdateNotificationByIdentifierAsync(
                notificationId,
                updatedNotification,
                cancellationToken);

        public async Task<ServiceResult> SendNotificationToUserAsync(
            Guid recipientAccountId,
            NotificationDTO notification,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(notification);

            DateTime timestamp = notification.Timestamp == default ? DateTime.UtcNow : notification.Timestamp;
            var notificationToPersist = new NotificationDTO
            {
                Id = NewNotificationId,
                Recipient = new UserDTO { Id = recipientAccountId },
                Timestamp = timestamp,
                Title = notification.Title,
                Body = notification.Body,
                Type = notification.Type,
                RelatedRequestId = notification.RelatedRequestId,
            };

            var persistResult = await apiNotificationService.PersistNotificationAsync(
                notificationToPersist,
                cancellationToken);
            if (!persistResult.Success)
            {
                return persistResult;
            }

            if (currentUserContext.CurrentUserId == recipientAccountId)
            {
                NotifyObservers(notificationToPersist);
                toastNotificationService.Show(notification.Title, notification.Body);
                return ServiceResult.Ok();
            }

            serverNotificationClient.SendNotification(
                ToServerInt(recipientAccountId),
                notification.Title,
                notification.Body);

            return ServiceResult.Ok();
        }

        public Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            apiNotificationService.GetNotificationsForUserAsync(accountId, cancellationToken);

        public Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(
            int relatedRequestId,
            CancellationToken cancellationToken = default) =>
            apiNotificationService.DeleteNotificationsLinkedToRequestAsync(relatedRequestId, cancellationToken);

        public void SubscribeToServer(Guid accountId) =>
            serverNotificationClient.SubscribeToServer(ToServerInt(accountId));

        public void StartListening() =>
            _ = Task.Run(async () =>
            {
                try
                {
                    await serverNotificationClient.ListenAsync();
                }
                catch (System.Net.Sockets.SocketException socketException)
                {
                    System.Diagnostics.Debug.WriteLine($"DesktopNotificationService: listen terminated - {socketException}");
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    System.Diagnostics.Debug.WriteLine($"DesktopNotificationService: listen terminated - {invalidOperationException}");
                }
            });

        public void StopListening() => serverNotificationClient.StopListening();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(IncomingNotification incomingNotification)
        {
            var notification = new NotificationDTO
            {
                Id = NewNotificationId,
                Recipient = new UserDTO { Id = Guid.Empty },
                Timestamp = incomingNotification.Timestamp,
                Title = incomingNotification.Title,
                Body = incomingNotification.Body,
            };

            NotifyObservers(notification);
            toastNotificationService.Show(incomingNotification.Title, incomingNotification.Body);
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer)
        {
            lock (notificationObserversLock)
            {
                notificationObservers.Add(observer);
            }

            return new NotificationObserverSubscription(
                notificationObservers,
                notificationObserversLock,
                observer);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            StopListening();
            (serverNotificationClient as IDisposable)?.Dispose();
        }

        private static int ToServerInt(Guid accountId) => Math.Abs(accountId.GetHashCode());

        private void NotifyObservers(NotificationDTO notification)
        {
            IObserver<NotificationDTO>[] observerSnapshot;
            lock (notificationObserversLock)
            {
                observerSnapshot = notificationObservers.ToArray();
            }

            foreach (var notificationObserver in observerSnapshot)
            {
                notificationObserver.OnNext(notification);
            }
        }

        private sealed class NotificationObserverSubscription : IDisposable
        {
            private readonly List<IObserver<NotificationDTO>> observers;
            private readonly object observersLock;
            private readonly IObserver<NotificationDTO> observerToRemove;

            public NotificationObserverSubscription(
                List<IObserver<NotificationDTO>> observers,
                object observersLock,
                IObserver<NotificationDTO> observerToRemove)
            {
                this.observers = observers;
                this.observersLock = observersLock;
                this.observerToRemove = observerToRemove;
            }

            public void Dispose()
            {
                lock (observersLock)
                {
                    observers.Remove(observerToRemove);
                }
            }
        }
    }
}
