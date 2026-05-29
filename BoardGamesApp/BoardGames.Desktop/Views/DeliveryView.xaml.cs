// <copyright file="DeliveryView.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Navigation;
using BoardGames.Desktop.Services;
using BoardGames.Shared.ProxyServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class DeliveryView : Page
    {
        private DeliveryNavigationArgs? navigationArgs;
        private Window hostWindow = null!;

        public DeliveryView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEvent)
        {
            base.OnNavigatedTo(navigationEvent);
            if (navigationEvent.Parameter is not DeliveryNavigationArgs args)
            {
                return;
            }

            this.navigationArgs = args;
            this.hostWindow = args.HostWindow;

            var accountService = App.Services.GetRequiredService<IAccountService>();
            var session = App.Services.GetRequiredService<ISessionContext>();
            var profile = await accountService.GetProfileAsync(session.AccountId);
            if (profile.Success && profile.Data != null)
            {
                this.CountryInput.Text = profile.Data.Country;
                this.CityInput.Text = profile.Data.City;
                this.StreetInput.Text = profile.Data.StreetName;
                this.StreetNumberInput.Text = profile.Data.StreetNumber;
            }
        }

        private void OnSubmitClicked(object sender, RoutedEventArgs e)
        {
            if (this.navigationArgs is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(this.CountryInput.Text) ||
                string.IsNullOrWhiteSpace(this.CityInput.Text) ||
                string.IsNullOrWhiteSpace(this.StreetInput.Text))
            {
                return;
            }

            string address = $"{this.CountryInput.Text.Trim()}, {this.CityInput.Text.Trim()}, {this.StreetInput.Text.Trim()} {this.StreetNumberInput.Text.Trim()}";
            var bookingArguments = new BookingNavigationArguments
            {
                RentalId = this.navigationArgs.Checkout.RentalId,
                ChatRequestId = this.navigationArgs.ChatRequestId,
                MessageId = this.navigationArgs.MessageId,
                Checkout = this.navigationArgs.Checkout,
                DeliveryAddress = address,
                CurrentWindow = this.hostWindow,
            };

            if (this.CashPaymentRadio.IsChecked == true)
            {
                this.Frame.Navigate(typeof(CashPaymentPage), bookingArguments);
            }
            else
            {
                this.Frame.Navigate(typeof(CardPaymentPage), bookingArguments);
            }
        }

        private void OnFieldChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void OnSaveAddressChecked(object sender, RoutedEventArgs e)
        {
        }

        private void OnSaveAddressUnchecked(object sender, RoutedEventArgs e)
        {
        }

        private void OnOpenMapClicked(object sender, RoutedEventArgs e)
        {
        }

        private void OnCloseMapClicked(object sender, RoutedEventArgs e)
        {
            this.MapOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnConfirmLocationClicked(object sender, RoutedEventArgs e)
        {
        }
    }
}
