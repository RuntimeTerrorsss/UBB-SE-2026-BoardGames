// <copyright file="PaymentAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BoardGames.Data.Repositories;

namespace BoardGames.Shared.ProxyRepositories
{
    public class PaymentAPIProxy : IPaymentRepository
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public PaymentAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync()
        {
            return await httpClient.GetFromJsonAsync<List<Payment>>("payments", JsonOptions)
                   ?? new List<Payment>();
        }

        public async Task<Payment?> GetPaymentByIdentifierAsync(int paymentId)
        {
            var response = await httpClient.GetAsync($"payments/{paymentId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Payment>(JsonOptions);
        }

        public async Task<int> AddPaymentAsync(Payment payment)
        {
            if (payment.DateOfTransaction == default)
            {
                payment.DateOfTransaction = DateTime.Now;
            }

            if (payment.TransactionIdentifier <= 0)
            {
                payment.TransactionIdentifier = 0;
            }

            payment.Request = null;
            payment.Client = null;
            payment.Owner = null;

            var response = await httpClient.PostAsJsonAsync("payments", payment, JsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Payment API returned {(int)response.StatusCode}: {errorBody}",
                    null,
                    response.StatusCode);
            }

            var newId = await response.Content.ReadFromJsonAsync<int>(JsonOptions);
            if (newId <= 0)
            {
                throw new InvalidOperationException("Payment API did not return a valid payment id.");
            }

            return newId;
        }

        public async Task<Payment?> UpdatePaymentAsync(Payment payment)
        {
            var response = await httpClient.PutAsJsonAsync(
                $"payments/{payment.TransactionIdentifier}",
                payment,
                JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Payment>(JsonOptions);
        }

        public async Task<bool> DeletePaymentAsync(Payment payment)
        {
            if (payment == null)
            {
                return false;
            }

            var response = await httpClient.DeleteAsync($"payments/{payment.TransactionIdentifier}");
            return response.IsSuccessStatusCode;
        }
    }
}
