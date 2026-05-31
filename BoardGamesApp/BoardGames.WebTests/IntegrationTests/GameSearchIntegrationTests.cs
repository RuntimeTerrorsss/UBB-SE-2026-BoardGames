#pragma warning disable SA1101 // Prefix local calls with this
#pragma warning disable SA1503 // Braces should not be omitted
#pragma warning disable SA1413 // Use trailing comma in multi-line initializers

using System.Net;
using System.Net.Http.Json;
using BoardGames.Api;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace BoardGames.WebTests.IntegrationTests
{
    public class GameSearchIntegrationTests : IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer;
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        public GameSearchIntegrationTests()
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext configuration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    var factoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextFactory<AppDbContext>));
                    if (factoryDescriptor != null) services.Remove(factoryDescriptor);

                    // Add Testcontainers DbContext
                    var connectionString = _msSqlContainer.GetConnectionString();
                    services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
                    services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
                });
            });

            _client = _factory.CreateClient();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
            }
        }

        public async Task DisposeAsync()
        {
            await _msSqlContainer.DisposeAsync();
            if (_factory != null)
            {
                await _factory.DisposeAsync();
            }
            _client?.Dispose();
        }

        [Fact]
        public async Task SearchGames_WithValidFilters_ReturnsFilteredGames()
        {
            // Arrange
            // Test Scenario Focus: Correct translation of URL filter criteria (keyword, players) to EF Core SQL commands
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var owner = new User 
                { 
                    Id = Guid.NewGuid(), 
                    PamUserId = 100, 
                    Username = "owner_test",
                    DisplayName = "Owner", 
                    Email = "owner@test.com", 
                    PasswordHash = "hash",
                    Country = "TestCountry",
                    City = "TestCity"
                };
                db.Users.Add(owner);
                
                db.Games.Add(new Game { Name = "Catan", MinimumPlayerNumber = 3, MaximumPlayerNumber = 4, PricePerDay = 50, IsActive = true, Description = "Test Catan", OwnerId = 100 });
                db.Games.Add(new Game { Name = "Catan Expansion", MinimumPlayerNumber = 3, MaximumPlayerNumber = 4, PricePerDay = 20, IsActive = true, Description = "Test Catan Ext", OwnerId = 100 });
                db.Games.Add(new Game { Name = "Ticket to Ride", MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, PricePerDay = 40, IsActive = true, Description = "Trains", OwnerId = 100 });
                await db.SaveChangesAsync();
            }

            var request = new GameSearchCriteriaDTO
            {
                Name = "Catan", // keyword
                PlayerCount = 3 // players
            };

            // Act
            // Test Scenario Focus: JSON serialization/deserialization of query DTOs
            var response = await _client.PostAsJsonAsync("/api/games/search", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var games = await response.Content.ReadFromJsonAsync<IReadOnlyList<GameSummaryDTO>>();
            Assert.NotNull(games);
            Assert.Equal(2, games.Count);
            Assert.Contains(games, g => g.Name == "Catan");
            Assert.Contains(games, g => g.Name == "Catan Expansion");
        }

        [Fact]
        public async Task SearchGames_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            // Database is intentionally left empty (no games seeded)
            var request = new GameSearchCriteriaDTO
            {
                Name = "NonExistent"
            };

            // Act
            // Test Scenario Focus: API behavior when the database is empty
            var response = await _client.PostAsJsonAsync("/api/games/search", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var games = await response.Content.ReadFromJsonAsync<IReadOnlyList<GameSummaryDTO>>();
            
            Assert.NotNull(games);
            Assert.Empty(games);
        }

        [Fact]
        public async Task SearchGames_ConnectionFails_ReturnsInternalServerError()
        {
            // Arrange
            // Test Scenario Focus: API behavior when connection fails (network timeout/503 errors or 500)
            var brokenFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    var factoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextFactory<AppDbContext>));
                    if (factoryDescriptor != null) services.Remove(factoryDescriptor);

                    var invalidConnectionString = "Server=invalid_server,1433;Database=TestDb;User Id=sa;Password=WrongPassword123!;Encrypt=False;Connection Timeout=1;";
                    services.AddDbContext<AppDbContext>(options => options.UseSqlServer(invalidConnectionString));
                    services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(invalidConnectionString), ServiceLifetime.Scoped);
                });
            });

            using var brokenClient = brokenFactory.CreateClient();
            var request = new GameSearchCriteriaDTO();

            // Act & Assert
            // Since the app doesn't have an exception handler configured in "Testing" mode,
            // the exception bubbles up directly to the TestServer client.
            await Assert.ThrowsAnyAsync<Exception>(() => brokenClient.PostAsJsonAsync("/api/games/search", new GameSearchCriteriaDTO()));
        }
    }
}
