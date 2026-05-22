namespace BoardGames.Desktop.Services
{
    public interface IToastNotificationService
    {
        void Show(string notificationTitle, string notificationBody);
    }
}