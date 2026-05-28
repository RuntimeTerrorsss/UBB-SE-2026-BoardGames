using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Microsoft.UI.Dispatching;

namespace BoardGames.Desktop.ViewModels
{
    public class NotificationsViewModel : PagedViewModel<NotificationDTO>, IObserver<NotificationDTO>, IDisposable
    {
        private readonly IDesktopNotificationService notificationLookupService;
        private readonly IDisposable notificationSubscription;
        private readonly ISessionContext sessionContext;
        private readonly IServerClient serverClient;

        private readonly DispatcherQueue? uiDispatcherQueue;
        private NotificationConnectionStatus currentConnectionStatus;

        public Guid CurrentUserId => sessionContext.AccountId;

        public bool HasConnectionWarning =>
            currentConnectionStatus == NotificationConnectionStatus.Offline
            || currentConnectionStatus == NotificationConnectionStatus.Reconnecting;

        public string ConnectionWarningMessage => currentConnectionStatus switch
        {
            NotificationConnectionStatus.Offline => "Notification server is offline. Live notifications are unavailable.",
            NotificationConnectionStatus.Reconnecting => "Reconnecting to the notification server...",
            _ => string.Empty,
        };

        public NotificationsViewModel(
            IDesktopNotificationService notificationLookupService,
            ISessionContext sessionContext,
            IServerClient serverClient)
        {
            this.notificationLookupService = notificationLookupService;
            this.sessionContext = sessionContext;
            this.serverClient = serverClient;
            this.currentConnectionStatus = serverClient.ConnectionStatus;

            try { uiDispatcherQueue = DispatcherQueue.GetForCurrentThread(); }
            catch (COMException) { uiDispatcherQueue = null; }

            this.serverClient.ConnectionStatusChanged += OnConnectionStatusChanged;

            if (sessionContext.IsLoggedIn)
            {
                _ = ReloadAsync();
            }
            notificationLookupService.SubscribeToServer(sessionContext.AccountId);
            notificationLookupService.StartListening();
        }

        protected override void Reload() => _ = ReloadAsync();

        private async Task ReloadAsync()
        {
            if (!sessionContext.IsLoggedIn) return;

            var notificationsResult = await notificationLookupService.GetNotificationsForUserAsync(this.sessionContext.AccountId);

            this.SetAllItems(notificationsResult.Success && notificationsResult.Data != null
                ? notificationsResult.Data.OrderByDescending(n => n.Id).ToImmutableList()
                : ImmutableList<NotificationDTO>.Empty);
        }

        public async Task DeleteNotificationByIdentifierAsync(int notificationIdToDelete)
        {
            await notificationLookupService.DeleteNotificationByIdentifierAsync(notificationIdToDelete);
            await ReloadAsync();
        }

        public void OnNext(NotificationDTO incomingNotification)
        {
            if (!sessionContext.IsLoggedIn) return;

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(() => _ = ReloadAsync());
                return;
            }
            _ = ReloadAsync();
        }

        public void OnCompleted() { }
        public void OnError(Exception error) => System.Diagnostics.Debug.WriteLine($"Notification error: {error.Message}");

        public void Dispose()
        {
            notificationSubscription?.Dispose();
            serverClient.ConnectionStatusChanged -= OnConnectionStatusChanged;
        }

        private void OnConnectionStatusChanged(object? sender, NotificationConnectionStatusChangedEventArgs eventArgs)
        {
            void ApplyStatus()
            {
                currentConnectionStatus = eventArgs.ConnectionStatus;
                OnPropertyChanged(nameof(HasConnectionWarning));
                OnPropertyChanged(nameof(ConnectionWarningMessage));
            }

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
                uiDispatcherQueue.TryEnqueue(ApplyStatus);
            else
                ApplyStatus();
        }
    }
}