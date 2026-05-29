// <copyright file="CashPaymentPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Navigation;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class CashPaymentPage : Page
    {
        private BookingNavigationArguments? booking;
        private Window? hostWindow;

        public CashPaymentPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);
            if (navigationArgs.Parameter is not BookingNavigationArguments args)
            {
                return;
            }

            this.booking = args;
            this.hostWindow = args.CurrentWindow;
            this.DataContext = new
            {
                GameName = args.Checkout.GameName,
                OwnerName = args.Checkout.OwnerName,
                RequestDates = args.Checkout.DateRange,
                DeliveryAddress = args.DeliveryAddress,
                PaidAmount = args.Checkout.Price.ToString("C"),
            };
        }

        private async void ConfirmCashPayment_Click(object sender, RoutedEventArgs e)
        {
            if (this.booking is null)
            {
                return;
            }

            var session = App.Services.GetRequiredService<ISessionContext>();
            var paymentService = App.Services.GetRequiredService<IRentalPaymentService>();
            var result = await paymentService.CompleteCardPaymentAsync(new CompleteRentalCardPaymentDTO
            {
                RequestId = this.booking.ChatRequestId,
                RentalId = this.booking.RentalId,
                MessageId = this.booking.MessageId,
                RenterAccountId = session.AccountId,
                PaymentMethod = "CASH",
            });

            if (!result.Success)
            {
                var dialog = new ContentDialog
                {
                    Title = "Payment failed",
                    Content = result.Error ?? "Could not complete the cash payment.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
                return;
            }

            this.hostWindow?.Close();
        }

        private void NavigateToChatButton_Click(object sender, RoutedEventArgs e)
        {
            this.hostWindow?.Close();
        }
    }
}
