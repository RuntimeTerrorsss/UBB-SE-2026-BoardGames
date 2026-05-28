// <copyright file="RequestsApiIntegrationTests.cs" company="BoardRent">
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
    public sealed class RequestsApiIntegrationTests
    {
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

            await ApiTestDataBuilder.SeedUserAsync(db, this.ownerId, 10, "owner", "owner@test.com");
            await ApiTestDataBuilder.SeedUserAsync(db, this.renterId, 11, "renter", "renter@test.com");

            this.gameId = await ApiTestDataBuilder.SeedGameAsync(db, 10, "Test Game");
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        private CreateRequestDTO CreateValidRequest()
        {
            return new CreateRequestDTO
            {
                GameId = this.gameId,
                OwnerAccountId = this.ownerId,
                RenterAccountId = this.renterId,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(5),
            };
        }

        [Test]
        public async Task CreateRequest_ValidData_ReturnsCreatedId()
        {
            var response = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task CreateRequest_InvalidDateRange_ReturnsBadRequest()
        {
            var dto = this.CreateValidRequest();
            dto.EndDate = dto.StartDate.AddDays(-1);

            var response = await this.client.PostAsJsonAsync("api/requests", dto);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CreateRequest_GameNotFound_ReturnsNotFound()
        {
            var dto = this.CreateValidRequest();
            dto.GameId = 999999;

            var response = await this.client.PostAsJsonAsync("api/requests", dto);
            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task CreateRequest_OwnerRentingOwnGame_ReturnsBadRequest()
        {
            var dto = this.CreateValidRequest();
            dto.RenterAccountId = this.ownerId;

            var response = await this.client.PostAsJsonAsync("api/requests", dto);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task ApproveRequest_ValidOwner_ReturnsSuccess()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var approve = new RequestActionDTO { AccountId = this.ownerId };
            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/approve", approve);
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ApproveRequest_UnauthorizedUser_ReturnsForbidden()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var approve = new RequestActionDTO { AccountId = Guid.NewGuid() };

            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/approve", approve);
            Assert.That((int)response.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task ApproveRequest_RequestMissing_ReturnsNotFound()
        {
            var approve = new RequestActionDTO { AccountId = this.ownerId };

            var response = await this.client.PutAsJsonAsync($"api/requests/999999/approve", approve);
            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DenyRequest_ValidOwner_ReturnsNoContent()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var deny = new RequestActionDTO
            {
                AccountId = this.ownerId,
                Reason = "Not available",
            };

            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/deny", deny);
            Assert.That((int)response.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task DenyRequest_UnauthorizedUser_ReturnsForbidden()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var deny = new RequestActionDTO
            {
                AccountId = Guid.NewGuid(),
            };

            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/deny", deny);
            Assert.That((int)response.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task CancelRequest_ValidRenter_ReturnsNoContent()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var cancel = new RequestActionDTO { AccountId = this.renterId };
            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/cancel", cancel);
            Assert.That((int)response.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task CancelRequest_UnauthorizedUser_ReturnsForbidden()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var cancel = new RequestActionDTO { AccountId = Guid.NewGuid() };

            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/cancel", cancel);
            Assert.That((int)response.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task OfferRequest_ValidOwner_ReturnsSuccess()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var offer = new RequestActionDTO { AccountId = this.ownerId };

            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/offer", offer);
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task OfferRequest_NotOwner_ReturnsForbidden()
        {
            var create = await this.client.PostAsJsonAsync("api/requests", this.CreateValidRequest());
            var created = await create.Content.ReadFromJsonAsync<dynamic>();
            int requestId = (int)created.id;

            var offer = new RequestActionDTO { AccountId = this.renterId };

            var response = await this.client.PutAsJsonAsync($"api/requests/{requestId}/offer", offer);
            Assert.That((int)response.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task CheckAvailability_ValidRange_ReturnsTrue()
        {
            var start = DateTime.UtcNow.AddDays(10);
            var end = DateTime.UtcNow.AddDays(12);

            var response = await this.client.GetAsync(
                $"api/requests/games/{this.gameId}/availability?startDate={start:o}&endDate={end:o}");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<bool>();
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GetBookedDates_ValidGame_ReturnsList()
        {
            var response = await this.client.GetAsync($"api/requests/games/{this.gameId}/booked-dates");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<BookedDateRangeDTO[]>();
            Assert.That(result, Is.Not.Null);
        }
    }
}
