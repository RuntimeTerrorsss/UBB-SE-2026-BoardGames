// <copyright file="DashboardApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Shared.DTO;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public class DashboardApiIntegrationTests
    {
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;
        private Guid accountId;

        [SetUp]
        public async Task Setup()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();

            this.client = this.factory.CreateClient();
            this.accountId = Guid.NewGuid();
            using var scope = this.factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(db, this.accountId, 100, "dashboard-user", "dash@test.com");

            db.Payments.Add(new HistoryPayment
            {
                TransactionIdentifier = 1,
                ClientId = 100,
                OwnerId = 200,
                GameName = "Chess",
                PaidAmount = 15,
                PaymentMethod = "Card",
                ReceiptFilePath = "file1.png",
                DateOfTransaction = DateTime.UtcNow.AddDays(-1),
                RentalStartDate = DateTime.UtcNow.AddDays(-3),
                RentalEndDate = DateTime.UtcNow.AddDays(-2),
            });

            db.Payments.Add(new HistoryPayment
            {
                TransactionIdentifier = 2,
                ClientId = 999,
                OwnerId = 888,
                GameName = "Monopoly",
                PaidAmount = 50,
                PaymentMethod = "Cash",
                ReceiptFilePath = "file2.png",
                DateOfTransaction = DateTime.UtcNow,
            });

            await db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task GetPaymentHistory_WithValidUser_ReturnsOk()
        {
            var response = await this.client.GetAsync($"api/payments/user/{this.accountId}/history");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var data = await response.Content.ReadFromJsonAsync<PaymentDTO[]>();

            Assert.That(data, Is.Not.Null);
        }

        [Test]
        public async Task GetPaymentHistory_WithInvalidUser_ReturnsEmptyList()
        {
            var response = await this.client.GetAsync($"api/payments/user/{Guid.NewGuid()}/history");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var data = await response.Content.ReadFromJsonAsync<PaymentDTO[]>();

            Assert.That(data!.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task GetPaymentHistory_ReturnsOnlyUserPayments()
        {
            var response = await this.client.GetAsync($"api/payments/user/{this.accountId}/history");

            var data = await response.Content.ReadFromJsonAsync<PaymentDTO[]>();

            Assert.That(data, Is.Not.Null);

            Assert.That(data!.All(payment => payment.PaymentId > 0), Is.True);
        }

        [Test]
        public async Task GetPaymentHistory_ReturnsSortedByDateDescending()
        {
            var response = await this.client.GetAsync($"api/payments/user/{this.accountId}/history");

            var data = await response.Content.ReadFromJsonAsync<PaymentDTO[]>();

            Assert.That(data, Is.Not.Null);

            for (int paymentIndex = 1; paymentIndex < data!.Length; paymentIndex++)
            {
                Assert.That(data[paymentIndex - 1].SortDate >= data[paymentIndex].SortDate);
            }
        }
    }
}
