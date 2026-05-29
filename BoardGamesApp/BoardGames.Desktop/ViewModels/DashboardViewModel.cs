namespace BoardGames.Desktop.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardGames.Desktop.Services;
    using BoardGames.Shared.DTO;
    using BoardGames.Shared.ProxyServices;

    public class DashboardViewModel : ViewModelBase
    {
        private readonly IConversationService conversationService;
        private readonly INotificationService notificationService;
        private readonly IPaymentService paymentService;
        private readonly ISessionContext sessionContext;

        private int newMessagesCount;
        private int pendingNotificationsCount;

        public int NewMessagesCount { get => newMessagesCount; set => SetProperty(ref newMessagesCount, value); }

        public int PendingNotificationsCount { get => pendingNotificationsCount; set => SetProperty(ref pendingNotificationsCount, value); }

        public ObservableCollection<PaymentDTO> RecentPayments { get; } = new();

        public DashboardViewModel(
            IConversationService conversationService,
            INotificationService notificationService,
            IPaymentService paymentService,
            ISessionContext sessionContext)
        {
            this.conversationService = conversationService;
            this.notificationService = notificationService;
            this.paymentService = paymentService;
            this.sessionContext = sessionContext;
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            if (!sessionContext.IsLoggedIn) return;

            var convResult = await conversationService.GetConversationsForUserAsync(sessionContext.AccountId);
            var notifResult = await notificationService.GetNotificationsForUserAsync(sessionContext.AccountId);
            var paymentsResult = await paymentService.GetFilteredPaymentsAsync(
                sessionContext.AccountId, FilterType.Newest, PaymentMethod.ALL, string.Empty, 1);

            if (convResult.Success && convResult.Data != null)
            {
                NewMessagesCount = convResult.Data.Count(c => c.UnreadCount.Values.Sum() > 0);
            }

            if (notifResult.Success && notifResult.Data != null)
            {
                PendingNotificationsCount = notifResult.Data.Count();
            }

            RecentPayments.Clear();
            if (paymentsResult.Success && paymentsResult.Data != null)
            {
                foreach (var payment in paymentsResult.Data.Items.Take(5))
                {
                    RecentPayments.Add(payment);
                }
            }
        }
    }
}