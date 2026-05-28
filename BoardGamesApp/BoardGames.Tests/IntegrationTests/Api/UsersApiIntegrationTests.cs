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
    public sealed class UsersApiIntegrationTests
    {
        private readonly Guid userAccountId = Guid.NewGuid();
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
            await ApiTestDataBuilder.SeedUserAsync(dbContext, userAccountId, 70, "chat-user", "chat-user@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task GetUsersExcept_ReturnsUsers()
        {
            var response = await client.GetAsync($"api/users/except/{userAccountId}");
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<UserDTO[]>();
            Assert.That(users, Is.Not.Null);
        }
    }
}
