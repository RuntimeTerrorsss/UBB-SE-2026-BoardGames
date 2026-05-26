// <copyright file="RepositoryPayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using System.Text.Json;
using BoardGames.Data.Repositories;

namespace BoardGames.Shared.ProxyRepositories
{
    public class RepositoryPaymentAPIProxy : IRepositoryPayment
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public RepositoryPaymentAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IReadOnlyList<HistoryPayment>> GetAllPayments()
        {
            return await httpClient.GetFromJsonAsync<List<HistoryPayment>>("payments/history", JsonOptions)
               ?? new List<HistoryPayment>();
        }

        public async Task<HistoryPayment?> GetPaymentById(int searchedPaymentId)
        {
            var response = await httpClient.GetAsync($"payments/history/{searchedPaymentId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<HistoryPayment>(JsonOptions);
        }
    }
}
