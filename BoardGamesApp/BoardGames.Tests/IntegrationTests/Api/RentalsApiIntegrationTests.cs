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
    public sealed class RentalsApiIntegrationTests
    {
        private readonly Guid ownerAccountId = Guid.NewGuid();
        private readonly Guid renterAccountId = Guid.NewGuid();
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            factory = new ApiWebApplicationFactory();
            await factory.EnsureDatabaseAsync();
            client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ApiTestDataBuilder.SeedUserAsync(dbContext, ownerAccountId, 60, "owner-rental", "owner-rental@example.com");
            await ApiTestDataBuilder.SeedUserAsync(dbContext, renterAccountId, 61, "renter-rental", "renter-rental@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task CreateRental_ThenGetForOwner_ReturnsRental()
        {
            int gameId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Rental Game");
            }

            var create = new CreateRentalDTO
            {
                GameId = gameId,
                OwnerAccountId = ownerAccountId,
                RenterAccountId = renterAccountId,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(7),
            };

            var createResponse = await client.PostAsJsonAsync("api/rentals", create);
            createResponse.EnsureSuccessStatusCode();

            var ownerResponse = await client.GetAsync($"api/rentals/owner/{ownerAccountId}");
            ownerResponse.EnsureSuccessStatusCode();
            var rentals = await ownerResponse.Content.ReadFromJsonAsync<RentalDTO[]>();

            Assert.That(rentals, Is.Not.Null);
            Assert.That(rentals!.Length, Is.EqualTo(1));
        }

        [Test]
        public async Task AvailabilityEndpoint_ReturnsFalseForExistingRental()
        {
            int gameId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Availability Game");

                dbContext.Rentals.Add(new Rental
                {
                    GameId = gameId,
                    ClientId = 61,
                    OwnerId = 60,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(4),
                });

                await dbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync($"api/rentals/games/{gameId}/availability?startDate={DateTime.UtcNow.AddDays(3):O}&endDate={DateTime.UtcNow.AddDays(4):O}");
            response.EnsureSuccessStatusCode();
            var available = await response.Content.ReadFromJsonAsync<bool>();

            Assert.That(available, Is.False);
        }
    }
}
