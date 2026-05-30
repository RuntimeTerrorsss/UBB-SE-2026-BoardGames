// <copyright file="CardPaymentViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using BoardGames.Desktop.Commands;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Desktop.ViewModels
{
    public class CardPaymentViewModel : INotifyPropertyChanged
    {
        private const double TimerBeforeClosingPayment = 30_000;
        private const double TimerForRefreshingBalance = 4000;
        private const int LoadingTime = 50;

        private readonly IRentalPaymentService rentalPaymentService;
        private readonly RentalCheckoutDTO checkout;
        private readonly CompleteRentalCardPaymentDTO paymentInfo;
        private readonly System.Timers.Timer inactivityTimer;
        private readonly System.Timers.Timer balanceRefreshTimer;
        private readonly SynchronizationContext? synchronizationContext;

        private decimal balanceAmount;
        private bool areTermsAccepted;
        private bool isCurrentlyLoading;
        private string currentStatusMessage = string.Empty;
        private bool isPaymentSuccessful;
        private string cardNumber = string.Empty;
        private string cardholderName = string.Empty;
        private string expiryDate = string.Empty;
        private string cardVerificationValue = string.Empty;
        private bool isPageCurrentlyActive;

        public CardPaymentViewModel(
            IRentalPaymentService rentalPaymentService,
            RentalCheckoutDTO checkout,
            CompleteRentalCardPaymentDTO paymentInfo,
            string deliveryAddress)
        {
            this.rentalPaymentService = rentalPaymentService;
            this.checkout = checkout;
            this.paymentInfo = paymentInfo;
            DeliveryAddress = deliveryAddress;
            FinishPaymentCommand = new RelayCommand(_ => { _ = FinishPaymentAsync(); }, () => IsPaymentButtonEnabled);
            ExitCommand = new RelayCommand(_ => NavigateBackwardsAction?.Invoke());
            ResetInactivityCommand = new RelayCommand(_ => ResetInactivityTimer());
            balanceRefreshTimer = new System.Timers.Timer(TimerForRefreshingBalance);
            balanceRefreshTimer.Elapsed += (timerSender, timerEventArguments) => RefreshBalance();
            balanceRefreshTimer.AutoReset = true;
            inactivityTimer = new System.Timers.Timer(TimerBeforeClosingPayment);
            inactivityTimer.Elapsed += OnSessionExpired;
            inactivityTimer.AutoReset = false;
            synchronizationContext = SynchronizationContext.Current;
        }

        public async Task InitializeAsync()
        {
            IsCurrentlyLoading = true;
            try
            {
                GameName = checkout.GameName;
                OwnerName = checkout.OwnerName;
                Price = checkout.Price;
                BalanceAmount = checkout.Balance;
                RequestDates = checkout.DateRange;
                DeliveryDate = checkout.DateRange;
            }
            finally
            {
                IsCurrentlyLoading = false;
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int RequestIdentifier => this.paymentInfo.RentalId;

        public int ClientIdentifier { get; private set; }

        public int OwnerIdentifier { get; private set; }

        public string GameName { get; private set; } = string.Empty;

        public string OwnerName { get; private set; } = string.Empty;

        public string ClientName { get; private set; } = string.Empty;

        public string DeliveryAddress { get; private set; } = string.Empty;

        public string DeliveryDate { get; private set; } = string.Empty;

        public string RequestDates { get; private set; } = string.Empty;

        public decimal Price { get; private set; }

        public int BookingMessageIdentifier => this.paymentInfo.MessageId;

        public decimal BalanceAmount
        {
            get => balanceAmount;
            set
            {
                if (balanceAmount == value) return;
                balanceAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                OnPropertyChanged(nameof(IsWarningMessageVisible));
            }
        }

        public bool AreTermsAccepted
        {
            get => areTermsAccepted;
            set
            {
                if (areTermsAccepted == value) return;
                areTermsAccepted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsCurrentlyLoading
        {
            get => isCurrentlyLoading;
            set
            {
                if (isCurrentlyLoading == value) return;
                isCurrentlyLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
            }
        }

        public string CurrentStatusMessage
        {
            get => currentStatusMessage;
            set
            {
                currentStatusMessage = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(CurrentStatusMessage);

        public bool IsPaymentSuccessful
        {
            get => isPaymentSuccessful;
            set
            {
                isPaymentSuccessful = value;
                OnPropertyChanged();
            }
        }

        public string CardNumber
        {
            get => cardNumber;
            set
            {
                if (cardNumber == value) return;
                cardNumber = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public string CardholderName
        {
            get => cardholderName;
            set
            {
                if (cardholderName == value) return;
                cardholderName = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public string ExpiryDate
        {
            get => expiryDate;
            set
            {
                if (expiryDate == value) return;
                expiryDate = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public string CardVerificationValue
        {
            get => cardVerificationValue;
            set
            {
                if (cardVerificationValue == value) return;
                cardVerificationValue = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsPaymentButtonEnabled =>
            AreTermsAccepted &&
            !IsCurrentlyLoading &&
            !string.IsNullOrWhiteSpace(CardNumber) &&
            !string.IsNullOrWhiteSpace(CardholderName) &&
            !string.IsNullOrWhiteSpace(ExpiryDate) &&
            !string.IsNullOrWhiteSpace(CardVerificationValue);

        public bool IsWarningMessageVisible => false;

        public RelayCommand FinishPaymentCommand { get; }

        public RelayCommand ExitCommand { get; }

        public RelayCommand ResetInactivityCommand { get; }

        public Action? NavigateBackwardsAction { get; set; }

        public Action? NavigateToExitAction { get; set; }

        public void OnPageActivated()
        {
            isPageCurrentlyActive = true;
            RefreshBalance();
            balanceRefreshTimer.Start();
            inactivityTimer.Start();
        }

        public void OnPageDeactivated()
        {
            isPageCurrentlyActive = false;
            balanceRefreshTimer.Stop();
            inactivityTimer.Stop();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void RefreshBalance()
        {
            if (!isPageCurrentlyActive)
            {
                return;
            }

            var refreshed = await this.rentalPaymentService.GetCheckoutSummaryAsync(this.paymentInfo.RentalId, this.paymentInfo.RenterAccountId);
            if (!refreshed.Success || refreshed.Data is null)
            {
                return;
            }

            synchronizationContext?.Post(
                _ =>
                {
                    BalanceAmount = refreshed.Data.Balance;
                    FinishPaymentCommand.NotifyCanExecuteChanged();
                }, null);
        }

        private async Task FinishPaymentAsync()
        {
            IsCurrentlyLoading = true;
            CurrentStatusMessage = string.Empty;
            FinishPaymentCommand.NotifyCanExecuteChanged();

            await Task.Delay(LoadingTime);

            try
            {
                this.paymentInfo.CardNumber = this.CardNumber;
                this.paymentInfo.CardholderName = this.CardholderName;
                this.paymentInfo.ExpiryDate = this.ExpiryDate;
                this.paymentInfo.CardVerificationValue = this.CardVerificationValue;
                this.paymentInfo.PaymentMethod = "CARD";

                var result = await this.rentalPaymentService.CompleteCardPaymentAsync(this.paymentInfo);
                if (!result.Success)
                {
                    throw new InvalidOperationException(result.Error ?? "Payment failed.");
                }

                IsPaymentSuccessful = true;
                CurrentStatusMessage = "Payment successful!";
                balanceRefreshTimer.Stop();
                inactivityTimer.Stop();
                OnPropertyChanged(nameof(IsPaymentSuccessful));

                await Task.Delay(1500);
                synchronizationContext?.Post(_ => NavigateToExitAction?.Invoke(), null);
            }
            catch (Exception paymentException)
            {
                CurrentStatusMessage = $"Payment failed: {paymentException.Message}";
            }
            finally
            {
                IsCurrentlyLoading = false;
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnSessionExpired(object? timerSender, System.Timers.ElapsedEventArgs elapsedEventArguments)
        {
            if (!isPageCurrentlyActive) return;

            balanceRefreshTimer.Stop();
            CurrentStatusMessage = "Session expired due to inactivity.";
            synchronizationContext?.Post(
                _ =>
                {
                    if (!isPageCurrentlyActive) return;
                    NavigateToExitAction?.Invoke();
                },
                null);
        }

        private void ResetInactivityTimer()
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }
    }
}
