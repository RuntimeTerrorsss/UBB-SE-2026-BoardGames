// <copyright file="IPaymentService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.ProxyServices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using BoardGames.Shared.DTO;

    public interface IPaymentService
    {
        Task<ServiceResult<PagedResult<PaymentDTO>>> GetFilteredPaymentsAsync(Guid accountId, FilterType filter, PaymentMethod method, string search, int page);

        Task<ServiceResult<string>> GetReceiptPathAsync(int paymentId);
    }

    public class PaymentService : ApiServiceBase, IPaymentService
    {
        public PaymentService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public async Task<ServiceResult<PagedResult<PaymentDTO>>> GetFilteredPaymentsAsync(Guid accountId, FilterType filter, PaymentMethod method, string search, int page)
        {
            var url = $"api/Payments/user/{accountId}/history?filter={filter}&method={method}&search={search}&page={page}";
            var response = await CreateClient().GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<PagedResult<PaymentDTO>>.Fail("Eroare API");
            }

            var data = await response.Content.ReadFromJsonAsync<List<PaymentDTO>>() ?? new List<PaymentDTO>();
            IEnumerable<PaymentDTO> filteredPayments = data;

            if (method != PaymentMethod.ALL)
            {
                filteredPayments = filteredPayments.Where(payment =>
                    string.Equals(payment.PaymentMethod, method.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredPayments = filteredPayments.Where(payment =>
                    (payment.ProductName ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (payment.ReceiverName ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (payment.OtherPartyName ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            filteredPayments = filter switch
            {
                FilterType.AlphabeticalAsc => filteredPayments.OrderBy(payment => payment.ProductName),
                FilterType.AlphabeticalDesc => filteredPayments.OrderByDescending(payment => payment.ProductName),
                FilterType.Oldest => filteredPayments.OrderBy(payment => payment.SortDate),
                FilterType.Newest => filteredPayments.OrderByDescending(payment => payment.SortDate),
                FilterType.Last3Months => filteredPayments.Where(payment => payment.SortDate >= DateTime.UtcNow.AddMonths(-3)),
                FilterType.Last6Months => filteredPayments.Where(payment => payment.SortDate >= DateTime.UtcNow.AddMonths(-6)),
                FilterType.Last9Months => filteredPayments.Where(payment => payment.SortDate >= DateTime.UtcNow.AddMonths(-9)),
                _ => filteredPayments,
            };

            const int pageSize = 10;
            var items = filteredPayments.ToList();
            var paged = new PagedResult<PaymentDTO>
            {
                Items = items.Skip(Math.Max(page - 1, 0) * pageSize).Take(pageSize).ToList(),
                TotalCount = items.Count,
                PageNumber = page,
                PageSize = pageSize,
            };

            return ServiceResult<PagedResult<PaymentDTO>>.Ok(paged);
        }

        public async Task<ServiceResult<string>> GetReceiptPathAsync(int paymentId)
            => await GetAsync<string>($"api/Payments/receipt/{paymentId}");
    }
}
