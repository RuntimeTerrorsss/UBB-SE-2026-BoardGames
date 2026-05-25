// <copyright file="CardPaymentViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Desktop.Commands;
using BoardGames.Data.Constants;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Api.Services;

namespace BoardGames.Desktop.ViewModels
{
    public class CardPaymentViewModel : INotifyPropertyChanged
    {
        private readonly ICardPaymentService cardPaymentService;
        private readonly IUserRepository userService;
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
            ICardPaymentService cardPaymentService,
            IUserRepository userService,
            int requestId,
            string deliveryAddress,
            int bookingMessageIdentifier,
            IConversationService conversationService)
        {
            this.cardPaymentService = cardPaymentService;
            this.userService = userService;
            RequestIdentifier = requestId;
            DeliveryAddress = deliveryAddress;
            BookingMessageIdentifier = bookingMessageIdentifier;
            ConversationService = conversationService;
            FinishPaymentCommand = new RelayCommand(_ => { _ = FinishPaymentAsync(); }, () => IsPaymentButtonEnabled);
            ExitCommand = new RelayCommand(_ => NavigateBackwardsAction?.Invoke());
            ResetInactivityCommand = new RelayCommand(_ => ResetInactivityTimer());
            balanceRefreshTimer = new System.Timers.Timer(CardPaymentConstants.TimerForRefreshingBalance);
            balanceRefreshTimer.Elapsed += (timerSender, timerEventArguments) => RefreshBalance();
            balanceRefreshTimer.AutoReset = true;
            inactivityTimer = new System.Timers.Timer(CardPaymentConstants.TimerBeforeClosingPayment);
            inactivityTimer.Elapsed += OnSessionExpired;
            inactivityTimer.AutoReset = false;
            synchronizationContext = SynchronizationContext.Current;
        }

        public async Task InitializeAsync()
        {
            IsCurrentlyLoading = true;
            try
            {
                RentalDataTransferObject requestDataTransferObject = await cardPaymentService.GetRequestDataTransferObject(RequestIdentifier);

                ClientIdentifier = requestDataTransferObject.ClientId;
                OwnerIdentifier = requestDataTransferObject.OwnerId;
                GameName = requestDataTransferObject.GameName;
                OwnerName = requestDataTransferObject.OwnerName;
                ClientName = requestDataTransferObject.ClientName;
                Price = requestDataTransferObject.Price;

                RequestDates = requestDataTransferObject.StartDate.ToShortDateString() + " to " + requestDataTransferObject.EndDate.ToShortDateString();
                DeliveryDate = requestDataTransferObject.StartDate.ToShortDateString();

                RefreshBalance();
            }
            finally
            {
                IsCurrentlyLoading = false;
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int RequestIdentifier { get; }

        public int ClientIdentifier { get; private set; }

        public int OwnerIdentifier { get; private set; }

        public string GameName { get; private set; } = string.Empty;

        public string OwnerName { get; private set; } = string.Empty;

        public string ClientName { get; private set; } = string.Empty;

        public string DeliveryAddress { get; private set; } = string.Empty;

        public string DeliveryDate { get; private set; } = string.Empty;

        public string RequestDates { get; private set; } = string.Empty;

        public decimal Price { get; private set; }

        public int BookingMessageIdentifier { get; }

        public IConversationService ConversationService { get; }

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
            }
        }

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
            BalanceAmount >= Price &&
            AreTermsAccepted &&
            !IsCurrentlyLoading &&
            !string.IsNullOrWhiteSpace(CardNumber) &&
            !string.IsNullOrWhiteSpace(CardholderName) &&
            !string.IsNullOrWhiteSpace(ExpiryDate) &&
            !string.IsNullOrWhiteSpace(CardVerificationValue);

        public bool IsWarningMessageVisible => BalanceAmount < Price;

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
            if (!isPageCurrentlyActive || ClientIdentifier == 0)
            {
                return;
            }

            decimal newBalance = await userService.GetUserBalance(ClientIdentifier);
            synchronizationContext?.Post(
                _ =>
                {
                    BalanceAmount = newBalance;
                    FinishPaymentCommand.NotifyCanExecuteChanged();
                }, null);
        }

        private async Task FinishPaymentAsync()
        {
            IsCurrentlyLoading = true;
            CurrentStatusMessage = string.Empty;
            FinishPaymentCommand.NotifyCanExecuteChanged();

            await Task.Delay(CardPaymentConstants.LoadingTime);

            try
            {
                await Task.Run(() =>
                    cardPaymentService.AddCardPayment(RequestIdentifier, ClientIdentifier, OwnerIdentifier, Price));

                await ((ConversationService)ConversationService).OnCardPaymentSelected(BookingMessageIdentifier);

                IsPaymentSuccessful = true;
                CurrentStatusMessage = "Payment successful!";
                RefreshBalance();
                balanceRefreshTimer.Stop();
                inactivityTimer.Stop();
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
