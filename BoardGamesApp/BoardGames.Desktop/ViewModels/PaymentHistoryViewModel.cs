namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BoardGames.Data.Enums;
    using BoardGames.Desktop.Commands;
    using BoardGames.Desktop.Services;
    using BoardGames.Shared.DTO;
    using BoardGames.Shared.ProxyServices;
    using Windows.Storage;
    using Windows.System;

    public class FilterOption
    {
        public FilterType Type { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }

    public class PaymentHistoryViewModel : ViewModelBase
    {
        private const int FirstPageNumber = 1;
        private const int InitialTotalPages = 1;
        private const int SearchDebounceMilliseconds = 300;

        private readonly IPaymentService paymentService;
        private readonly ISessionContext sessionContext;

        private FilterOption selectedFilterOption;
        private PaymentMethod selectedPaymentMethod;
        private string searchText = string.Empty;
        private decimal totalAmount;
        private int currentPage = FirstPageNumber;
        private int totalPages = InitialTotalPages;
        private bool isLoading;
        private string errorMessage = string.Empty;
        private CancellationTokenSource? searchCancellationTokenSource;

        public ObservableCollection<PaymentDTO> Payments { get; } = new();

        public ObservableCollection<FilterOption> FilterOptions { get; }

        public IEnumerable<PaymentMethod> PaymentMethodOptions { get; } = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>();

        public RelayCommand<PaymentDTO> OpenReceiptCommand { get; }

        public RelayCommandNoParam NextPageCommand { get; }

        public RelayCommandNoParam PreviousPageCommand { get; }

        public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

        public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

        public decimal TotalAmount { get => totalAmount; private set => SetProperty(ref totalAmount, value); }

        public int CurrentPage { get => currentPage; set => SetProperty(ref currentPage, value); }

        public int TotalPages { get => totalPages; set => SetProperty(ref totalPages, value); }

        public string SearchText
        {
            get => searchText;
            set { if (SetProperty(ref searchText, value)) TriggerDebouncedSearch(); }
        }

        public FilterOption SelectedFilterOption
        {
            get => selectedFilterOption;
            set { if (SetProperty(ref selectedFilterOption, value)) _ = ApplyFilter(true); }
        }

        public PaymentMethod SelectedPaymentMethod
        {
            get => selectedPaymentMethod;
            set { if (SetProperty(ref selectedPaymentMethod, value)) _ = ApplyFilter(true); }
        }

        public PaymentHistoryViewModel(IPaymentService paymentService, ISessionContext sessionContext)
        {
            this.paymentService = paymentService;
            this.sessionContext = sessionContext;

            FilterOptions = new ObservableCollection<FilterOption>
            {
                new() { Type = FilterType.AllTime, DisplayName = "All Time" },
                new() { Type = FilterType.Newest, DisplayName = "Date: Newest First" },
                new() { Type = FilterType.Oldest, DisplayName = "Date: Oldest First" },
                new() { Type = FilterType.AlphabeticalAsc, DisplayName = "Alphabetical (A-Z)" }
            };

            selectedFilterOption = FilterOptions.First();
            selectedPaymentMethod = PaymentMethod.ALL;

            OpenReceiptCommand = new RelayCommand<PaymentDTO>(async (dto) => await OpenReceipt(dto));
            NextPageCommand = new RelayCommandNoParam(async () => await OnNextPage(), () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommandNoParam(async () => await OnPreviousPage(), () => CurrentPage > 1);

            _ = ApplyFilter(true);
        }

        private void TriggerDebouncedSearch()
        {
            searchCancellationTokenSource?.Cancel();
            searchCancellationTokenSource = new CancellationTokenSource();
            var token = searchCancellationTokenSource.Token;
            _ = Task.Delay(SearchDebounceMilliseconds, token).ContinueWith(async _ => await ApplyFilter(true), token);
        }

        private async Task OnNextPage() { CurrentPage++; await ApplyFilter(false); }

        private async Task OnPreviousPage() { CurrentPage--; await ApplyFilter(false); }

        public async Task ApplyFilter(bool resetPage)
        {
            if (!sessionContext.IsLoggedIn) return;
            if (resetPage) CurrentPage = FirstPageNumber;

            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await paymentService.GetFilteredPaymentsAsync(
                sessionContext.AccountId,
                selectedFilterOption.Type,
                selectedPaymentMethod,
                searchText,
                CurrentPage);

            if (result.Success && result.Data != null)
            {
                Payments.Clear();
                foreach (var payment in result.Data.Items)
                {
                    Payments.Add(payment);
                }

                TotalPages = result.Data.TotalPages;
                TotalAmount = Payments.Sum(payment => payment.Amount);

                NextPageCommand.RaiseCanExecuteChanged();
                PreviousPageCommand.RaiseCanExecuteChanged();
            }
            else
            {
                ErrorMessage = result.Error ?? "Eroare la Ã®ncÄƒrcarea plÄƒÈ›ilor.";
            }

            IsLoading = false;
        }

        private async Task OpenReceipt(PaymentDTO selectedPayment)
        {
            if (selectedPayment == null) return;
            var result = await paymentService.GetReceiptPathAsync(selectedPayment.PaymentId);

            if (result.Success && !string.IsNullOrEmpty(result.Data))
            {
                try
                {
                    var storageFile = await StorageFile.GetFileFromPathAsync(result.Data);
                    await Launcher.LaunchFileAsync(storageFile);
                }
                catch (Exception)
                {
                    ErrorMessage = "Nu s-a putut deschide chitanÈ›a.";
                }
            }
        }
    }
}
