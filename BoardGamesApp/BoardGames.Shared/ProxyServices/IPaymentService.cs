namespace BoardGames.Shared.ProxyServices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using BoardGames.Data.Enums;
    using BoardGames.Shared.DTO;

    public interface IPaymentService
    {
        Task<ServiceResult<PagedResult<PaymentDTO>>> GetFilteredPaymentsAsync(Guid accountId, FilterType filter, PaymentMethod method, string search, int page);
        Task<ServiceResult<string>> GetReceiptPathAsync(int paymentId);
    }

    public class PaymentService : ApiServiceBase, IPaymentService
    {
        public PaymentService(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<ServiceResult<PagedResult<PaymentDTO>>> GetFilteredPaymentsAsync(Guid accountId, FilterType filter, PaymentMethod method, string search, int page)
        {
            var url = $"api/Payments/user/{accountId}/history?filter={filter}&method={method}&search={search}&page={page}";
            var response = await CreateClient().GetAsync(url);

            if (!response.IsSuccessStatusCode) return ServiceResult<PagedResult<PaymentDTO>>.Fail("Eroare API");

            var data = await response.Content.ReadFromJsonAsync<PagedResult<PaymentDTO>>();
            return ServiceResult<PagedResult<PaymentDTO>>.Ok(data!);
        }

        public async Task<ServiceResult<string>> GetReceiptPathAsync(int paymentId)
            => await GetAsync<string>($"api/Payments/receipt/{paymentId}");
    }
}