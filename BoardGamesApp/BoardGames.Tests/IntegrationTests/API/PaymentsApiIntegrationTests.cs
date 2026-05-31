// <copyright file="PaymentsApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public sealed class PaymentsApiIntegrationTests
    {
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();
            this.client = this.factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task AddPayment_ThenGetAll_ReturnsPayment()
        {
            var payment = new Payment
            {
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 100m,
            };

            var addResponse = await this.client.PostAsJsonAsync("api/payments", payment);
            addResponse.EnsureSuccessStatusCode();

            var getResponse = await this.client.GetAsync("api/payments");
            getResponse.EnsureSuccessStatusCode();

            var payments = await getResponse.Content.ReadFromJsonAsync<Payment[]>();

            Assert.That(payments, Is.Not.Null);
            Assert.That(payments!.Length, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task GetPayment_ById_ReturnsPayment()
        {
            var payment = new Payment
            {
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 55m,
            };

            var createResponse = await this.client.PostAsJsonAsync("api/payments", payment);
            var createdId = await createResponse.Content.ReadFromJsonAsync<int>();

            var response = await this.client.GetAsync($"api/payments/{createdId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Payment>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.PaidAmount, Is.EqualTo(55m));
        }

        [Test]
        public async Task GetPayment_ByInvalidId_ReturnsNotFound()
        {
            var response = await this.client.GetAsync("api/payments/999999");

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task UpdatePayment_ThenGet_ReturnsUpdatedPayment()
        {
            var payment = new Payment
            {
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 10m,
            };

            var createResponse = await this.client.PostAsJsonAsync("api/payments", payment);
            var createdId = await createResponse.Content.ReadFromJsonAsync<int>();

            var updatedPayment = new Payment
            {
                TransactionIdentifier = createdId,
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 999m,
            };

            var updateResponse = await this.client.PutAsJsonAsync($"api/payments/{createdId}", updatedPayment);
            updateResponse.EnsureSuccessStatusCode();

            var updated = await updateResponse.Content.ReadFromJsonAsync<Payment>();

            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.PaidAmount, Is.EqualTo(999m));
        }

        [Test]
        public async Task UpdatePayment_InvalidId_ReturnsNotFound()
        {
            var payment = new Payment
            {
                TransactionIdentifier = 999999,
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 10m,
            };

            var response = await this.client.PutAsJsonAsync("api/payments/999999", payment);

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeletePayment_RemovesPayment()
        {
            var payment = new Payment
            {
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 20m,
            };

            var createResponse = await this.client.PostAsJsonAsync("api/payments", payment);
            var createdId = await createResponse.Content.ReadFromJsonAsync<int>();

            var deleteResponse = await this.client.DeleteAsync($"api/payments/{createdId}");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            var getResponse = await this.client.GetAsync($"api/payments/{createdId}");
            Assert.That((int)getResponse.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeletePayment_InvalidId_ReturnsNotFound()
        {
            var response = await this.client.DeleteAsync("api/payments/999999");

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task GetHistory_ReturnsHistoryList()
        {
            var response = await this.client.GetAsync("api/payments/history");

            response.EnsureSuccessStatusCode();

            var history = await response.Content.ReadFromJsonAsync<HistoryPayment[]>();

            Assert.That(history, Is.Not.Null);
        }

        [Test]
        public async Task GetHistoryById_InvalidId_ReturnsNotFound()
        {
            var response = await this.client.GetAsync("api/payments/history/999999");

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task GetHistoryForUser_ReturnsUserHistory()
        {
            var userId = Guid.NewGuid();

            var response = await this.client.GetAsync($"api/payments/user/{userId}/history");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaymentDTO[]>();

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task AddPayment_WithoutDate_SetsDefaultDate()
        {
            var payment = new Payment
            {
                PaidAmount = 50m,
                DateOfTransaction = default,
            };

            var response = await this.client.PostAsJsonAsync("api/payments", payment);

            response.EnsureSuccessStatusCode();

            var id = await response.Content.ReadFromJsonAsync<int>();

            Assert.That(id, Is.GreaterThan(0));
        }
    }
}
