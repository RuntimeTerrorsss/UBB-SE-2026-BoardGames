// <copyright file="PaymentHistoryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;
using BookingBoardGames.Sharing.DTO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class PaymentHistoryView : Page
    {
        public PaymentHistoryViewModel ViewModel { get; }

        public PaymentHistoryView()
        {
            this.InitializeComponent();
            this.ViewModel = new PaymentHistoryViewModel(App.ServicePayment!);
        }

        public PaymentHistoryView(PaymentHistoryViewModel viewModel)
        {
            this.InitializeComponent();
            this.ViewModel = viewModel;
        }

        public void OnReceiptButtonClicked(object sender, RoutedEventArgs routedArgs)
        {
            if (sender is Button clickedButton && clickedButton.DataContext is PaymentDataTransferObject selectedPayment)
            {
                if (this.ViewModel.OpenReceiptCommand != null && this.ViewModel.OpenReceiptCommand.CanExecute(selectedPayment))
                {
                    this.ViewModel.OpenReceiptCommand.Execute(selectedPayment);
                }
            }

            // fallback for null
            else if (sender is Button fallbackButton && fallbackButton.Tag is PaymentDataTransferObject fallbackPayment)
            {
                if (this.ViewModel.OpenReceiptCommand != null && this.ViewModel.OpenReceiptCommand.CanExecute(fallbackPayment))
                {
                    this.ViewModel.OpenReceiptCommand.Execute(fallbackPayment);
                }
            }
        }

        public void OnBackToDashboardClicked(object sender, RoutedEventArgs routedArgs)
        {
            var currentParentElement = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
            while (currentParentElement != null && !(currentParentElement is Frame))
            {
                currentParentElement = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(currentParentElement);
            }

            if (currentParentElement is Frame navigationFrame)
            {
                navigationFrame.Navigate(typeof(DashboardView));
            }
        }
    }
}
