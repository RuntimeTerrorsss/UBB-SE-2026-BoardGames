
using BoardGames.Desktop.Navigation;
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class CardPaymentPage : Page
    {
        private Window activeCurrentWindow;

        public CardPaymentViewModel PaymentViewModel { get; set; }

        public CardPaymentPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs navigationEventArguments)
        {
            base.OnNavigatedTo(navigationEventArguments);
            var bookingArguments = (BookingNavigationArguments)navigationEventArguments.Parameter;

            this.PaymentViewModel = new CardPaymentViewModel(
                App.CardPaymentService,
                App.UserRepository,
                bookingArguments.RequestIdentifier,
                bookingArguments.DeliveryAddress,
                bookingArguments.BookingMessageIdentifier,
                bookingArguments.ConversationService);
            await this.PaymentViewModel.InitializeAsync();

            this.DataContext = this.PaymentViewModel;
            this.activeCurrentWindow = bookingArguments.CurrentWindow;

            this.Bindings.Update();

            this.PaymentViewModel.NavigateBackwardsAction = () =>
            {
                this.activeCurrentWindow.Close();
            };
            this.PaymentViewModel.NavigateToExitAction = () =>
            {
                this.activeCurrentWindow.Close();
            };

            this.PaymentViewModel.OnPageActivated();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs onNavigatedFromEventArguments)
        {
            base.OnNavigatedFrom(onNavigatedFromEventArguments);
            this.PaymentViewModel.OnPageDeactivated();
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs onPointerMovedEventArguments)
        {
            base.OnPointerMoved(onPointerMovedEventArguments);
            this.PaymentViewModel.ResetInactivityCommand.Execute(null);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs onKeyDownEventArguments)
        {
            base.OnKeyDown(onKeyDownEventArguments);
            this.PaymentViewModel.ResetInactivityCommand.Execute(null);
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
