// <copyright file="CashPaymentPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BookingBoardGames.Src.Navigation;
using BookingBoardGames.Src.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class CashPaymentPage : Page
    {
        public CashPaymentViewModel PaymentViewModel { get; set; }

        private Window currentApplicationWindow;

        public CashPaymentPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);

            try
            {
                if (navigationArgs.Parameter is BookingNavigationArguments booking)
                {
                    if (booking.ConversationService == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Cash payment navigation missing conversation service.");
                        return;
                    }

                    this.PaymentViewModel = new CashPaymentViewModel(
                        App.CashPaymentService,
                        App.UserRepository,
                        App.RentalService,
                        App.GameRepository,
                        booking.RequestIdentifier,
                        booking.DeliveryAddress,
                        booking.BookingMessageIdentifier,
                        booking.ConversationService);

                    await this.PaymentViewModel.InitializeAsync(
                        booking.RequestIdentifier,
                        booking.DeliveryAddress);

                    this.DataContext = this.PaymentViewModel;
                    this.currentApplicationWindow = booking.CurrentWindow;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void NavigateToChatButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            if (this.currentApplicationWindow != null)
            {
                this.currentApplicationWindow.Close();
                return;
            }

            if (this.Frame?.CanGoBack == true)
            {
                this.Frame.GoBack();
            }
        }
    }
}
