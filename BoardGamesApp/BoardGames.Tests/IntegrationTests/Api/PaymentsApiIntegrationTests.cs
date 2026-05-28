using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
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
            factory = new ApiWebApplicationFactory();
            await factory.EnsureDatabaseAsync();
            client = factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task AddPayment_ThenGetAll_ReturnsPayment()
        {
            var payment = new Payment
            {
                DateOfTransaction = DateTime.UtcNow,
                PaidAmount = 100m,
            };

            var addResponse = await client.PostAsJsonAsync("api/payments", payment);
            addResponse.EnsureSuccessStatusCode();

            var getResponse = await client.GetAsync("api/payments");
            getResponse.EnsureSuccessStatusCode();
            var payments = await getResponse.Content.ReadFromJsonAsync<Payment[]>();

            Assert.That(payments, Is.Not.Null);
            Assert.That(payments!.Length, Is.GreaterThanOrEqualTo(1));
        }
    }
}
