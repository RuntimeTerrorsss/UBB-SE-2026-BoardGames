namespace BoardGames.Desktop.Services
{
    public class ToastNotificationService : IToastNotificationService
    {
        public void Show(string notificationTitle, string notificationBody)
        {
            System.Diagnostics.Debug.WriteLine($"Notification: {notificationTitle} - {notificationBody}");
        }
    }
}
