// <copyright file="ResultAndErrorEnumsIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
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
    public sealed class ResultAndErrorEnumsIntegrationTests
    {
        private sealed class CreatedRequestResponse
        {
            public int Id { get; set; }
        }
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        private Guid ownerId;
        private Guid renterId;
        private int gameId;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();
            this.client = this.factory.CreateClient();

            using var scope = this.factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            this.ownerId = Guid.NewGuid();
            this.renterId = Guid.NewGuid();

            await ApiTestDataBuilder.SeedUserAsync(db, this.ownerId, 20, "owner2", "o@test.com");
            await ApiTestDataBuilder.SeedUserAsync(db, this.renterId, 21, "renter2", "r@test.com");
            this.gameId = await ApiTestDataBuilder.SeedGameAsync(db, 20, "Result Game");
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        private CreateRequestDTO Valid() => new()
        {
            GameId = this.gameId,
            OwnerAccountId = this.ownerId,
            RenterAccountId = this.renterId,
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(6),
        };

        [Test]
        public async Task Result_Success_ReturnsOk()
        {
            var res = await this.client.PostAsJsonAsync("api/requests", this.Valid());
            res.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task Result_GameMissing_ReturnsNotFound()
        {
            var dto = this.Valid();
            dto.GameId = 999999;

            var res = await this.client.PostAsJsonAsync("api/requests", dto);
            Assert.That((int)res.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Result_InvalidDate_ReturnsBadRequest()
        {
            var dto = this.Valid();
            dto.EndDate = dto.StartDate.AddDays(-10);

            var res = await this.client.PostAsJsonAsync("api/requests", dto);
            Assert.That((int)res.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Result_OwnerCannotRent_ReturnsBadRequest()
        {
            var dto = this.Valid();
            dto.RenterAccountId = this.ownerId;

            var res = await this.client.PostAsJsonAsync("api/requests", dto);
            Assert.That((int)res.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Enum_ApproveUnauthorized_ReturnsForbidden()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.Valid());
            var created = await create.Content.ReadFromJsonAsync<CreatedRequestResponse>();
            int id = created!.Id;

            var res = await this.client.PutAsJsonAsync(
                $"api/requests/{id}/approve",
                new RequestActionDTO { AccountId = Guid.NewGuid() });

            Assert.That((int)res.StatusCode, Is.EqualTo(403));
        }
    }
}
