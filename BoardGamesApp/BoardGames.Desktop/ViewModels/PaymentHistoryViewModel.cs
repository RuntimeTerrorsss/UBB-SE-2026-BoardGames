// <copyright file="PaymentHistoryViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.ViewModels
{
    public class FilterOption
    {
        public FilterType Type { get; set; }

        public string DisplayName { get; set; }
    }

    public class PaymentHistoryViewModel : ViewModelBase
    {
        private readonly IServicePayment paymentService;
        private FilterOption selectedFilterOption;
        private PaymentMethod selectedPaymentMethod;
        private string searchText = string.Empty;
        private CancellationTokenSource searchCancellationTokenSource;
        private decimal totalAmount;

        private int currentPage = PaymentHistoryViewModelConstants.FirstPage;
        private int pageSize = PaymentHistoryViewModelConstants.DefaultPageSize;
        private int totalPages = PaymentHistoryViewModelConstants.StartupTotalPagesCount;

        private const int MinimumPageCount = PaymentHistoryViewModelConstants.MinimumPagesCount;

        public ObservableCollection<PaymentDataTransferObject> Payments { get; set; }

        public RelayCommand<PaymentDataTransferObject> OpenReceiptCommand { get; }

        public RelayCommandNoParam NextPageCommand { get; }

        public RelayCommandNoParam PreviousPageCommand { get; }

        public int CurrentPage
        {
            get => this.currentPage;
            set
            {
                if (this.SetProperty(ref this.currentPage, value))
                {
                    this.NextPageCommand?.RaiseCanExecuteChanged();
                    this.PreviousPageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPages
        {
            get => this.totalPages;
            set
            {
                if (this.SetProperty(ref this.totalPages, value))
                {
                    this.NextPageCommand?.RaiseCanExecuteChanged();
                    this.PreviousPageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<FilterOption> FilterOptions { get; }

        public IEnumerable<PaymentMethod> PaymentMethodOptions { get; } = System.Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>();

        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (this.SetProperty(ref this.searchText, value))
                {
                    this.searchCancellationTokenSource?.Cancel();
                    this.searchCancellationTokenSource = new CancellationTokenSource();
                    _ = this.DebounceSearch(this.searchCancellationTokenSource.Token);
                }
            }
        }

        private async Task DebounceSearch(CancellationToken searchCancellationToken)
        {
            try
            {
                await Task.Delay(PaymentHistoryViewModelConstants.TaskDelayTime, searchCancellationToken);
                if (!searchCancellationToken.IsCancellationRequested)
                {
                    await this.ApplyFilter(resetPage: true);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        public FilterOption SelectedFilterOption
        {
            get => this.selectedFilterOption;
            set
            {
                if (this.SetProperty(ref this.selectedFilterOption, value))
                {
                    _ = this.ApplyFilter(resetPage: true);
                }
            }
        }

        public PaymentMethod SelectedPaymentMethod
        {
            get => this.selectedPaymentMethod;
            set
            {
                if (this.SetProperty(ref this.selectedPaymentMethod, value))
                {
                    _ = this.ApplyFilter(resetPage: true);
                }
            }
        }

        public decimal TotalAmount
        {
            get => this.totalAmount;
            private set
            {
                if (this.SetProperty(ref this.totalAmount, value))
                {
                    this.OnPropertyChanged(nameof(this.TotalAmountText));
                }
            }
        }

        public string TotalAmountText => $"{this.TotalAmount:C}";

        public PaymentHistoryViewModel(IServicePayment paymentService)
        {
            this.paymentService = paymentService;
            this.Payments = new ObservableCollection<PaymentDataTransferObject>();

            this.FilterOptions = new ObservableCollection<FilterOption>
            {
                new FilterOption { Type = FilterType.AllTime, DisplayName = "All Time" },
                new FilterOption { Type = FilterType.Last3Months, DisplayName = "Last 3 Months" },
                new FilterOption { Type = FilterType.Last6Months, DisplayName = "Last 6 Months" },
                new FilterOption { Type = FilterType.Last9Months, DisplayName = "Last 9 Months" },
                new FilterOption { Type = FilterType.Newest, DisplayName = "Date: Newest First" },
                new FilterOption { Type = FilterType.Oldest, DisplayName = "Date: Oldest First" },
                new FilterOption { Type = FilterType.AlphabeticalAsc, DisplayName = "Alphabetical (A-Z)" },
                new FilterOption { Type = FilterType.AlphabeticalDesc, DisplayName = "Alphabetical (Z-A)" },
            };

            this.OpenReceiptCommand = new RelayCommand<PaymentDataTransferObject>(async dto => await this.OpenReceipt(dto));
            this.NextPageCommand = new RelayCommandNoParam(async () => await this.OnNextPage(), () => this.CurrentPage < this.TotalPages);
            this.PreviousPageCommand = new RelayCommandNoParam(async () => await this.OnPreviousPage(), () => this.CurrentPage > PaymentHistoryViewModelConstants.FirstPage);

            // Default to display all
            this.SelectedFilterOption = this.FilterOptions.First(filter => filter.Type == FilterType.AllTime);
            this.SelectedPaymentMethod = PaymentMethod.ALL;
        }

        private bool OnLastPage()
        {
            return this.CurrentPage == this.TotalPages;
        }

        private async Task OnNextPage()
        {
            if (!this.OnLastPage())
            {
                this.CurrentPage++;
                await this.ApplyFilter(resetPage: false);
            }
        }

        private bool OnFirstPage()
        {
            return this.CurrentPage == PaymentHistoryViewModelConstants.FirstPage;
        }

        private async Task OnPreviousPage()
        {
            if (!this.OnFirstPage())
            {
                this.CurrentPage--;
                await this.ApplyFilter(resetPage: false);
            }
        }

        private async Task OpenReceipt(PaymentDataTransferObject selectedPayment)
        {
            if (selectedPayment == null)
            {
                return;
            }

            string receiptFilePath = await this.paymentService.GetReceiptDocumentPath(selectedPayment.PaymentId);

            try
            {
                var receiptFileInfo = new System.IO.FileInfo(receiptFilePath);
                if (receiptFileInfo.Exists)
                {
                    // windows storage file reference to launch safely
                    var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(receiptFileInfo.FullName);
                    await Windows.System.Launcher.LaunchFileAsync(storageFile);
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task ApplyFilter(bool resetPage = false)
        {
            if (this.selectedFilterOption == null)
            {
                return;
            }

            if (resetPage)
            {
                this.CurrentPage = PaymentHistoryViewModelConstants.FirstPage;
            }

            var pagedResult = await this.paymentService.GetFilteredPayments(this.selectedFilterOption.Type, this.selectedPaymentMethod, this.searchText, this.CurrentPage, this.pageSize);

            this.Payments.Clear();
            foreach (var payment in pagedResult.Items)
            {
                this.Payments.Add(payment);
            }

            this.TotalPages = pagedResult.TotalPages == PaymentHistoryViewModelConstants.NoPages ? MinimumPageCount : pagedResult.TotalPages;

            this.TotalAmount = this.paymentService.CalculateTotalAmount(pagedResult.Items);
        }
    }
}
