// <copyright file="CardPaymentPage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Navigation;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class CardPaymentPage : Page
    {
        private Window activeCurrentWindow = null!;

        public CardPaymentViewModel PaymentViewModel { get; set; } = null!;

        public CardPaymentPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArguments)
        {
            base.OnNavigatedTo(navigationEventArguments);
            if (navigationEventArguments.Parameter is not BookingNavigationArguments bookingArguments)
            {
                return;
            }

            var session = App.Services.GetRequiredService<ISessionContext>();
            this.PaymentViewModel = new CardPaymentViewModel(
                App.Services.GetRequiredService<IRentalPaymentService>(),
                bookingArguments.Checkout,
                new CompleteRentalCardPaymentDTO
                {
                    RequestId = bookingArguments.ChatRequestId,
                    RentalId = bookingArguments.RentalId,
                    MessageId = bookingArguments.MessageId,
                    RenterAccountId = session.AccountId,
                },
                bookingArguments.DeliveryAddress);

            await this.PaymentViewModel.InitializeAsync();
            this.DataContext = this.PaymentViewModel;
            this.activeCurrentWindow = bookingArguments.CurrentWindow;
            this.Bindings.Update();

            this.PaymentViewModel.NavigateBackwardsAction = () => this.activeCurrentWindow.Close();
            this.PaymentViewModel.NavigateToExitAction = () => this.activeCurrentWindow.Close();
            this.PaymentViewModel.OnPageActivated();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs onNavigatedFromEventArguments)
        {
            base.OnNavigatedFrom(onNavigatedFromEventArguments);
            this.PaymentViewModel?.OnPageDeactivated();
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs onPointerMovedEventArguments)
        {
            base.OnPointerMoved(onPointerMovedEventArguments);
            this.PaymentViewModel?.ResetInactivityCommand.Execute(null);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs onKeyDownEventArguments)
        {
            base.OnKeyDown(onKeyDownEventArguments);
            this.PaymentViewModel?.ResetInactivityCommand.Execute(null);
        }

        private async void OnTermsLinkClick(Hyperlink hyperlinkSender, HyperlinkClickEventArgs onTermsClickedEventArguments)
        {
            var termsDialog = new ContentDialog
            {
                Title = "Terms of Service",
                Content = "By completing this payment you agree to our refund policy. " +
                          "Rentals are non-refundable once the rental period has started.",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot,
            };
            await termsDialog.ShowAsync();
        }
    }
}
