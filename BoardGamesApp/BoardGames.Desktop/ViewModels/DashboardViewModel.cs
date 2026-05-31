// <copyright file="DashboardViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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
        private readonly IRentalService rentalService;
        private readonly IRequestService requestService;
        private readonly IPaymentService paymentService;
        private readonly ISessionContext sessionContext;

        public DashboardViewModel(
            IRentalService rentalService,
            IRequestService requestService,
            IPaymentService paymentService,
            ISessionContext sessionContext)
        {
            this.rentalService = rentalService;
            this.requestService = requestService;
            this.paymentService = paymentService;
            this.sessionContext = sessionContext;
            _ = LoadDashboardDataAsync();
        }

        public ObservableCollection<RentalDTO> UpcomingRentals { get; } = new();

        public ObservableCollection<RequestDTO> OpenRequests { get; } = new();

        public ObservableCollection<PaymentDTO> RecentPayments { get; } = new();

        private async Task LoadDashboardDataAsync()
        {
            if (!sessionContext.IsLoggedIn)
            {
                return;
            }

            var accountId = sessionContext.AccountId;

            var rentalsTask = rentalService.GetRentalsForRenterAsync(accountId);
            var requestsTask = requestService.GetOpenRequestsForOwnerAsync(accountId);
            var paymentsTask = paymentService.GetFilteredPaymentsAsync(
                accountId, FilterType.Newest, PaymentMethod.ALL, string.Empty, 1);

            await Task.WhenAll(rentalsTask, requestsTask, paymentsTask);

            UpcomingRentals.Clear();
            if (rentalsTask.Result.Success && rentalsTask.Result.Data != null)
            {
                foreach (var rental in rentalsTask.Result.Data
                    .Where(currentRental => !currentRental.IsExpired)
                    .OrderBy(currentRental => currentRental.StartDate)
                    .Take(5))
                {
                    UpcomingRentals.Add(rental);
                }
            }

            OpenRequests.Clear();
            if (requestsTask.Result.Success && requestsTask.Result.Data != null)
            {
                foreach (var request in requestsTask.Result.Data.Take(5))
                {
                    OpenRequests.Add(request);
                }
            }

            RecentPayments.Clear();
            if (paymentsTask.Result.Success && paymentsTask.Result.Data != null)
            {
                foreach (var payment in paymentsTask.Result.Data.Items.Take(5))
                {
                    RecentPayments.Add(payment);
                }
            }
        }
    }
}
