// <copyright file="RentalsApiIntegrationTests.cs" company="BoardRent">
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
    public sealed class RentalsApiIntegrationTests
    {
        private readonly Guid ownerAccountId = Guid.NewGuid();
        private readonly Guid renterAccountId = Guid.NewGuid();

        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();
            this.client = this.factory.CreateClient();
            using var scope = this.factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(dbContext, this.ownerAccountId, 60, "owner", "owner@test.com");
            await ApiTestDataBuilder.SeedUserAsync(dbContext, this.renterAccountId, 61, "renter", "renter@test.com");
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task CreateRental_ThenGetForOwner_ReturnsRental()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Game 1");
            }

            var createRental = new CreateRentalDTO
            {
                GameId = gameId,
                OwnerAccountId = this.ownerAccountId,
                RenterAccountId = this.renterAccountId,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(6),
            };

            var createResponse = await this.client.PostAsJsonAsync("api/rentals", createRental);
            createResponse.EnsureSuccessStatusCode();

            var ownerResponse = await this.client.GetAsync($"api/rentals/owner/{this.ownerAccountId}");
            ownerResponse.EnsureSuccessStatusCode();

            var rentals = await ownerResponse.Content.ReadFromJsonAsync<RentalDTO[]>();

            Assert.That(rentals, Is.Not.Null);
            Assert.That(rentals!.Length, Is.EqualTo(1));
        }

        [Test]
        public async Task GetRentalsForRenter_ReturnsRentals()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Game 2");

                dbContext.Rentals.Add(new Rental
                {
                    GameId = gameId,
                    ClientId = 61,
                    OwnerId = 60,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(3),
                });

                await dbContext.SaveChangesAsync();
            }

            var response = await this.client.GetAsync($"api/rentals/renter/{this.renterAccountId}");
            response.EnsureSuccessStatusCode();

            var rentals = await response.Content.ReadFromJsonAsync<RentalDTO[]>();

            Assert.That(rentals, Is.Not.Null);
            Assert.That(rentals!.Length, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task AvailabilityEndpoint_ReturnsFalse_WhenOverlappingRentalExists()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Game 3");

                dbContext.Rentals.Add(new Rental
                {
                    GameId = gameId,
                    ClientId = 61,
                    OwnerId = 60,
                    StartDate = DateTime.UtcNow.AddDays(5),
                    EndDate = DateTime.UtcNow.AddDays(6),
                });

                await dbContext.SaveChangesAsync();
            }

            var response = await this.client.GetAsync(
                $"api/rentals/games/{gameId}/availability?startDate={DateTime.UtcNow.AddDays(5):O}&endDate={DateTime.UtcNow.AddDays(6):O}");

            response.EnsureSuccessStatusCode();

            var available = await response.Content.ReadFromJsonAsync<bool>();

            Assert.That(available, Is.False);
        }

        [Test]
        public async Task AvailabilityEndpoint_ReturnsTrue_WhenNoConflictsExist()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Game 4");
            }

            var response = await this.client.GetAsync(
                $"api/rentals/games/{gameId}/availability?startDate={DateTime.UtcNow.AddDays(10):O}&endDate={DateTime.UtcNow.AddDays(11):O}");

            response.EnsureSuccessStatusCode();

            var available = await response.Content.ReadFromJsonAsync<bool>();

            Assert.That(available, Is.True);
        }

        [Test]
        public async Task CreateRental_InvalidDateRange_ReturnsBadRequest()
        {
            var invalidRental = new CreateRentalDTO
            {
                GameId = 999,
                OwnerAccountId = this.ownerAccountId,
                RenterAccountId = this.renterAccountId,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(1),
            };

            var response = await this.client.PostAsJsonAsync("api/rentals", invalidRental);

            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CreateRental_WrongOwner_ReturnsConflict()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Game 5");
            }

            var request = new CreateRentalDTO
            {
                GameId = gameId,
                OwnerAccountId = Guid.NewGuid(),
                RenterAccountId = this.renterAccountId,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(3),
            };

            var response = await this.client.PostAsJsonAsync("api/rentals", request);

            Assert.That((int)response.StatusCode, Is.EqualTo(409));
        }

        [Test]
        public async Task CreateRental_GameNotFound_ReturnsNotFound()
        {
            var request = new CreateRentalDTO
            {
                GameId = 999999,
                OwnerAccountId = this.ownerAccountId,
                RenterAccountId = this.renterAccountId,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(3),
            };

            var response = await this.client.PostAsJsonAsync("api/rentals", request);

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task CreateRental_SlotAlreadyTaken_ReturnsConflict()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 60, "Game 6");

                dbContext.Rentals.Add(new Rental
                {
                    GameId = gameId,
                    ClientId = 61,
                    OwnerId = 60,
                    StartDate = DateTime.UtcNow.AddDays(5),
                    EndDate = DateTime.UtcNow.AddDays(6),
                });

                await dbContext.SaveChangesAsync();
            }

            var request = new CreateRentalDTO
            {
                GameId = gameId,
                OwnerAccountId = this.ownerAccountId,
                RenterAccountId = this.renterAccountId,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(6),
            };

            var response = await this.client.PostAsJsonAsync("api/rentals", request);

            Assert.That((int)response.StatusCode, Is.EqualTo(409));
        }
    }
}
