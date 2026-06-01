#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1633 // The file header is missing
#pragma warning disable SA1518 // File is required to end with a single newline character
#pragma warning disable SA1028 // Code should not contain trailing whitespace
#pragma warning disable SA1513 // Closing brace should be followed by blank line
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.MsSql;
using Xunit;


namespace BoardGames.WebTests.IntegrationTests
{
    public class SharedDatabaseFixture : IAsyncLifetime
    {
        public MsSqlContainer MsSqlContainer { get; private set; } = null!;
        public WebApplicationFactory<Program> Factory { get; private set; } = null!;
        public HttpClient Client { get; private set; } = null!;
        public Mock<INotificationService> MockNotificationService { get; private set; } = null!;
        public Mock<IConversationApiService> MockConversationApiService { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            MsSqlContainer = new MsSqlBuilder().Build();
            await MsSqlContainer.StartAsync();

            var connectionString = MsSqlContainer.GetConnectionString();
            MockNotificationService = new Mock<INotificationService>();
            MockConversationApiService = new Mock<IConversationApiService>();

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    var factoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextFactory<AppDbContext>));
                    if (factoryDescriptor != null) services.Remove(factoryDescriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseSqlServer(connectionString);
                    }, ServiceLifetime.Scoped);
                    
                    services.AddScoped(_ => MockNotificationService.Object);
                    services.AddScoped(_ => MockConversationApiService.Object);
                });
            });

            Client = Factory.CreateClient();

            using var scope = Factory.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = dbFactory.CreateDbContext();
            await db.Database.MigrateAsync();

            var owner = new User 
            { 
                Id = Guid.NewGuid(), 
                PamUserId = 200, 
                Username = "request_owner",
                DisplayName = "Owner", 
                Email = "owner@test.com", 
                PasswordHash = "hash",
                Country = "TestCountry",
                City = "TestCity"
            };

            var renter = new User 
            { 
                Id = Guid.NewGuid(), 
                PamUserId = 201, 
                Username = "request_renter",
                DisplayName = "Renter", 
                Email = "renter@test.com", 
                PasswordHash = "hash",
                Country = "TestCountry",
                City = "TestCity"
            };

            db.Users.AddRange(owner, renter);
            
            db.Games.Add(new Game 
            { 
                Name = "Monopoly", 
                MinimumPlayerNumber = 2, 
                MaximumPlayerNumber = 8, 
                PricePerDay = 15, 
                IsActive = true, 
                Description = "Test Monopoly", 
                OwnerId = 200 
            });
            
            await db.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            Client?.Dispose();
            if (Factory != null)
            {
                await Factory.DisposeAsync();
            }
            if (MsSqlContainer != null)
            {
                await MsSqlContainer.DisposeAsync();
            }
        }
    }

    [Collection("SharedDatabase")]
    public class RequestCreationIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public RequestCreationIntegrationTests(SharedDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateRequest_WithValidData_CreatesPendingRequestInDatabase()
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = dbFactory.CreateDbContext();
            
            var game = await db.Games.FirstAsync(g => g.Name == "Monopoly");
            var renter = await db.Users.FirstAsync(u => u.PamUserId == 201);
            var owner = await db.Users.FirstAsync(u => u.PamUserId == 200);

            var request = new CreateRequestDTO
            {
                GameId = game.Id,
                RenterAccountId = renter.Id,
                OwnerAccountId = owner.Id,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var response = await _fixture.Client.PostAsJsonAsync("/api/requests", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var savedRequest = await db.Requests.FirstOrDefaultAsync(r => r.Game!.Id == game.Id);
            Assert.NotNull(savedRequest);
            Assert.Equal(BoardGames.Data.Enums.RequestStatus.Open, savedRequest.Status);
            Assert.Equal(request.StartDate.Date, savedRequest.StartDate.Date);
            Assert.Equal(request.EndDate.Date, savedRequest.EndDate.Date);
        }

        [Fact]
        public async Task CreateRequest_WithInvalidDates_ReturnsBadRequest()
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = dbFactory.CreateDbContext();
            
            var game = await db.Games.FirstAsync(g => g.Name == "Monopoly");
            var renter = await db.Users.FirstAsync(u => u.PamUserId == 201);
            var owner = await db.Users.FirstAsync(u => u.PamUserId == 200);

            var request = new CreateRequestDTO
            {
                GameId = game.Id,
                RenterAccountId = renter.Id,
                OwnerAccountId = owner.Id,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(5) 
            };

            var response = await _fixture.Client.PostAsJsonAsync("/api/requests", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateRequest_WithNonExistentGame_ReturnsNotFound()
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = dbFactory.CreateDbContext();
            
            var renter = await db.Users.FirstAsync(u => u.PamUserId == 201);
            var owner = await db.Users.FirstAsync(u => u.PamUserId == 200);

            var request = new CreateRequestDTO
            {
                GameId = 9999,
                RenterAccountId = renter.Id,
                OwnerAccountId = owner.Id,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var response = await _fixture.Client.PostAsJsonAsync("/api/requests", request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
