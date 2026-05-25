// <copyright file="DeliveryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.Validators;
using BookingBoardGames.Src.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class DeliveryView : Page
    {
        private DeliveryViewModel deliveryViewModel;

        private double pendingLatitude;
        private double pendingLongitude;

        private int currentUserId;
        private int requestId;
        private int incomingMessageId;
        private ConversationService conversationService;
        private Window currentWindow;

        public DeliveryView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);

            var args = ((int UserId, int RequestId, int MessageId, ConversationService ConversationService, Window ToWindow))navigationArgs.Parameter;

            this.currentUserId = args.UserId;
            this.requestId = args.RequestId;
            this.incomingMessageId = args.MessageId;
            this.conversationService = args.ConversationService;
            this.currentWindow = args.ToWindow;
            this.deliveryViewModel = new DeliveryViewModel(
                this.currentUserId,
                App.MapService,
                App.UserRepository,
                new AddressValidator());

            this.deliveryViewModel.OnNavigateToPayment = () =>
            {
                var bookingArguments = new BookingNavigationArguments
                {
                    RequestIdentifier = this.requestId,
                    DeliveryAddress = this.deliveryViewModel.CurrentAddress.Country + ", " +
                                      this.deliveryViewModel.CurrentAddress.City + ", " +
                                      this.deliveryViewModel.CurrentAddress.Street + " " +
                                      this.deliveryViewModel.CurrentAddress.StreetNumber,
                    BookingMessageIdentifier = this.incomingMessageId,
                    ConversationService = this.conversationService,
                    CurrentWindow = this.currentWindow,
                };

                if (this.CashPaymentRadio.IsChecked == true)
                {
                    this.Frame.Navigate(typeof(CashPaymentPage), bookingArguments);
                }
                else
                {
                    this.Frame.Navigate(typeof(CardPaymentPage), bookingArguments);
                }
            };

            this.deliveryViewModel.StateChanged += this.RefreshUi;
            _ = this.deliveryViewModel.InitializeAsync();
            this.RefreshUi();
        }

        private void RefreshUi()
        {
            this.CountryInput.Text = this.deliveryViewModel.CurrentAddress.Country;
            this.CityInput.Text = this.deliveryViewModel.CurrentAddress.City;
            this.StreetInput.Text = this.deliveryViewModel.CurrentAddress.Street;
            this.StreetNumberInput.Text = this.deliveryViewModel.CurrentAddress.StreetNumber;

            this.MapOverlay.Visibility = this.deliveryViewModel.IsMapVisible
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.ShowFieldError(this.CountryInput, this.CountryError, "Country");
            this.ShowFieldError(this.CityInput, this.CityError, "City");
            this.ShowFieldError(this.StreetInput, this.StreetError, "Street");
            this.ShowFieldError(this.StreetNumberInput, this.StreetNumberError, "StreetNumber");
        }

        private void ShowFieldError(TextBox input, TextBlock errorBlock, string fieldName)
        {
            if (this.deliveryViewModel.ValidationErrors.TryGetValue(fieldName, out string message))
            {
                errorBlock.Text = message;
                errorBlock.Visibility = Visibility.Visible;
            }
            else
            {
                errorBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void OnFieldChanged(object sender, TextChangedEventArgs textArgs)
        {
            if (sender is TextBox textBox && textBox.Tag is string fieldName)
            {
                this.deliveryViewModel.OnFieldChange(fieldName, textBox.Text);
            }
        }

        private void OnSaveAddressChecked(object sender, RoutedEventArgs routedArgs)
            => this.deliveryViewModel.OnSaveAddressChanged(true);

        private void OnSaveAddressUnchecked(object sender, RoutedEventArgs routedArgs)
            => this.deliveryViewModel.OnSaveAddressChanged(false);

        private void OnOpenMapClicked(object sender, RoutedEventArgs routedArgs)
            => _ = this.InitializeMapAsync();

        private void OnCloseMapClicked(object sender, RoutedEventArgs routedArgs)
            => this.deliveryViewModel.CloseMap();

        private async void OnSubmitClicked(object sender, RoutedEventArgs routedArgs)
            => await this.deliveryViewModel.SubmitDelivery();

        private async void OnConfirmLocationClicked(object sender, RoutedEventArgs routedArgs)
            => await this.deliveryViewModel.ConfirmMapLocationAsync(this.pendingLatitude, this.pendingLongitude);

        private async Task InitializeMapAsync()
        {
            this.deliveryViewModel.OpenMap();
            await this.MapWebView.EnsureCoreWebView2Async();

            this.MapWebView.CoreWebView2.WebMessageReceived -= this.OnMapMessageReceived;
            this.MapWebView.CoreWebView2.WebMessageReceived += this.OnMapMessageReceived;

            this.MapWebView.CoreWebView2.NavigateToString("""
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
<style>html, body, #map { height: 100%; margin: 0; padding: 0; }</style>
</head>
<body>
<div id="map"></div>
<script>
var map = L.map('map').setView([46.7712, 23.5897], 13);
var marker = null;
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
map.on('click', function(e) {
if (marker) marker.setLatLng(e.latlng);
else marker = L.marker(e.latlng).addTo(map);
window.chrome.webview.postMessage(JSON.stringify({ lat: e.latlng.lat, lng: e.latlng.lng }));
});
</script>
</body>
</html>
""");
        }

        private void OnMapMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs messageArgs)
        {
            var message = messageArgs.TryGetWebMessageAsString();
            using var document = JsonDocument.Parse(message);

            this.pendingLatitude = document.RootElement.GetProperty("lat").GetDouble();
            this.pendingLongitude = document.RootElement.GetProperty("lng").GetDouble();
        }
    }
}
