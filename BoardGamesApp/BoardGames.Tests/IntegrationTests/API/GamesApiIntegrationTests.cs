// <copyright file="GamesApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
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
    public sealed class GamesApiIntegrationTests
    {
        private readonly Guid ownerAccountId = Guid.NewGuid();

        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();

            await this.factory.EnsureDatabaseAsync();

            this.client = this.factory.CreateClient();

            using var scope = this.factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(
                dbContext,
                this.ownerAccountId,
                99,
                "game-owner",
                "owner@test.com");
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task GetAll_WhenGamesExist_ReturnsGames()
        {
            using var scope = this.factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

            var response = await this.client.GetAsync("api/games");

            response.EnsureSuccessStatusCode();

            var games =
                await response.Content.ReadFromJsonAsync<GameSummaryDTO[]>();

            Assert.That(games, Is.Not.Null);
            Assert.That(games!.Length, Is.GreaterThan(0));
        }

        [Test]
        public async Task GetById_WhenGameExists_ReturnsGame()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);
            }

            var response =
                await this.client.GetAsync($"api/games/{gameId}");

            response.EnsureSuccessStatusCode();

            var game =
                await response.Content.ReadFromJsonAsync<GameDetailDTO>();

            Assert.That(game, Is.Not.Null);
            Assert.That(game!.Id, Is.EqualTo(gameId));
        }

        [Test]
        public async Task GetById_WhenGameDoesNotExist_ReturnsNotFound()
        {
            var response =
                await this.client.GetAsync("api/games/99999");

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Create_WhenDataIsValid_ReturnsCreated()
        {
            var dto = new GameCreateDTO
            {
                Name = "Catan",
                Description = "Very fun game description",
                Price = 10,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                OwnerAccountId = this.ownerAccountId,
                Image = Array.Empty<byte>(),
            };

            var response =
                await this.client.PostAsJsonAsync("api/games", dto);

            Assert.That((int)response.StatusCode, Is.EqualTo(201));
        }

        [Test]
        public async Task Create_WhenDataIsInvalid_ReturnsBadRequest()
        {
            var dto = new GameCreateDTO
            {
                Name = string.Empty,
                Description = string.Empty,
                Price = -1,
                MinimumPlayerNumber = 0,
                MaximumPlayerNumber = 0,
                OwnerAccountId = this.ownerAccountId,
            };

            var response =
                await this.client.PostAsJsonAsync("api/games", dto);

            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Update_WhenUserIsOwner_ReturnsNoContent()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);
            }

            var dto = new GameUpdateDTO
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 99,
                MinimumPlayerNumber = 1,
                MaximumPlayerNumber = 8,
                Image = Array.Empty<byte>(),
            };

            var response = await this.client.PutAsJsonAsync(
                $"api/games/{gameId}?requestingAccountId={this.ownerAccountId}",
                dto);

            Assert.That((int)response.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task Update_WhenUserIsNotOwner_ReturnsForbidden()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);
            }

            var dto = new GameUpdateDTO
            {
                Name = "Updated",
                Description = "Updated Description",
                Price = 10,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
            };

            var response = await this.client.PutAsJsonAsync(
                $"api/games/{gameId}?requestingAccountId={Guid.NewGuid()}",
                dto);

            Assert.That((int)response.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task Update_WhenDataIsInvalid_ReturnsBadRequest()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);
            }

            var dto = new GameUpdateDTO
            {
                Name = string.Empty,
                Description = string.Empty,
                Price = -1,
                MinimumPlayerNumber = 0,
                MaximumPlayerNumber = 0,
            };

            var response = await this.client.PutAsJsonAsync(
                $"api/games/{gameId}?requestingAccountId={this.ownerAccountId}",
                dto);

            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Update_WhenGameDoesNotExist_ReturnsInternalServerError()
        {
            var dto = new GameUpdateDTO
            {
                Name = "Updated",
                Description = "Valid Description",
                Price = 10,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
            };

            var response = await this.client.PutAsJsonAsync(
                $"api/games/99999?requestingAccountId={this.ownerAccountId}",
                dto);

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Delete_WhenUserIsOwner_ReturnsOk()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);
            }

            var response = await this.client.DeleteAsync(
                $"api/games/{gameId}?requestingAccountId={this.ownerAccountId}");

            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task Delete_WhenUserIsNotOwner_ReturnsForbidden()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);
            }

            var response = await this.client.DeleteAsync(
                $"api/games/{gameId}?requestingAccountId={Guid.NewGuid()}");

            Assert.That((int)response.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task Delete_WhenGameDoesNotExist_ReturnsInternalServerError()
        {
            var response = await this.client.DeleteAsync(
                $"api/games/99999?requestingAccountId={this.ownerAccountId}");

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Delete_WhenGameHasActiveRentals_ReturnsBadRequest()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId =
                    await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

                dbContext.Rentals.Add(new Rental
                {
                    GameId = gameId,
                    ClientId = 99,
                    OwnerId = 99,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5),
                });

                await dbContext.SaveChangesAsync();
            }

            var response = await this.client.DeleteAsync(
                $"api/games/{gameId}?requestingAccountId={this.ownerAccountId}");

            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Search_WhenMatchingGamesExist_ReturnsGames()
        {
            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await ApiTestDataBuilder
                    .SeedGameAsync(dbContext, 99, "Chess");
            }

            var dto = new GameSearchCriteriaDTO
            {
                Name = "Chess",
                SortBy = "PriceAscending",
            };

            var response = await this.client.PostAsJsonAsync(
                "api/games/search",
                dto);

            response.EnsureSuccessStatusCode();

            var games =
                await response.Content.ReadFromJsonAsync<GameSummaryDTO[]>();

            Assert.That(games, Is.Not.Null);
            Assert.That(games!.Any(game => game.Name == "Chess"), Is.True);
        }

        [Test]
        public async Task Search_WhenSortOptionIsInvalid_DoesNotFail()
        {
            var dto = new GameSearchCriteriaDTO
            {
                SortBy = "INVALID_ENUM",
            };

            var response = await this.client.PostAsJsonAsync(
                "api/games/search",
                dto);

            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task GetImage_WhenImageExists_ReturnsImage()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

                var game = dbContext.Games.First(game => game.Id == gameId);

                game.Image = new byte[] { 1, 2, 3 };

                await dbContext.SaveChangesAsync();
            }

            var response = await this.client.GetAsync($"api/games/{gameId}/image");

            response.EnsureSuccessStatusCode();

            Assert.That(
                response.Content.Headers.ContentType!.MediaType,
                Is.EqualTo("image/jpeg"));
        }

        [Test]
        public async Task GetImage_WhenImageDoesNotExist_ReturnsNotFound()
        {
            int gameId;

            using (var scope = this.factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                gameId = await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

                var game = dbContext.Games.First(game => game.Id == gameId);

                game.Image = Array.Empty<byte>();

                await dbContext.SaveChangesAsync();
            }

            var response = await this.client.GetAsync($"api/games/{gameId}/image");

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task GetByOwner_WhenGamesExist_ReturnsGames()
        {
            using var scope = this.factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

            var response = await this.client.GetAsync($"api/games/owner/{this.ownerAccountId}");

            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task GetActiveByOwner_WhenGamesExist_ReturnsGames()
        {
            using var scope = this.factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

            var response = await this.client.GetAsync($"api/games/owner/{this.ownerAccountId}/active");

            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task GetAllGamesAdmin_WhenGamesExist_ReturnsGames()
        {
            using var scope = this.factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedGameAsync(dbContext, 99);

            var response = await this.client.GetAsync("api/games/admin");

            response.EnsureSuccessStatusCode();
        }
    }
}
