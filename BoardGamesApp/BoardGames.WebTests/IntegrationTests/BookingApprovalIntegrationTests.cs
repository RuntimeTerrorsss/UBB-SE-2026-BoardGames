#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1633 // The file header is missing
#pragma warning disable SA1518 // File is required to end with a single newline character
#pragma warning disable SA1028 // Code should not contain trailing whitespace
#pragma warning disable SA1513 // Closing brace should be followed by blank line
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Mappers;
using BoardGames.Api.Services;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoardGames.WebTests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class BookingApprovalIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public BookingApprovalIntegrationTests(SharedDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<(RequestService Service, AppDbContext Db)> CreateTestSubject(IServiceScope scope)
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var dbContext = dbFactory.CreateDbContext();

            // Use the real repositories!
            var requestRepo = new RequestRepository(dbFactory);
            var rentalRepo = new RentalRepository(dbContext);
            var gameRepo = new GamesRepository(dbContext);

            // Mock only external dependencies (like Notifications/Conversations)
            var notificationServiceMock = new Mock<INotificationService>();
            var conversationApiServiceMock = new Mock<IConversationApiService>();

            var mapper = scope.ServiceProvider.GetRequiredService<RequestMapper>();

            var service = new RequestService(
                requestRepo,
                rentalRepo,
                gameRepo,
                notificationServiceMock.Object,
                conversationApiServiceMock.Object,
                mapper);

            return (service, dbContext);
        }

        [Fact]
        public async Task ApproveRequest_ExecutesAtomically_CreatesRentalAndDeletesRequest()
        {
            // Arrange
            using var scope = _fixture.Factory.Services.CreateScope();
            var (service, dbContext) = await CreateTestSubject(scope);

            var random = new Random();
            var ownerIdStr = Guid.NewGuid().ToString("N");
            var renterIdStr = Guid.NewGuid().ToString("N");
            var owner = new User { Id = Guid.NewGuid(), DisplayName = "Owner", PamUserId = random.Next(10000, 99999), Username = "owner" + ownerIdStr, Email = ownerIdStr + "@test.com", PasswordHash = "hash", Country = "TestCountry", City = "TestCity" };
            var renter = new User { Id = Guid.NewGuid(), DisplayName = "Renter", PamUserId = random.Next(10000, 99999), Username = "renter" + renterIdStr, Email = renterIdStr + "@test.com", PasswordHash = "hash", Country = "TestCountry", City = "TestCity" };
            var game = new Game { Name = "Test Game", Owner = owner, IsActive = true };
            
            var request = new Request 
            { 
                Game = game, 
                Owner = owner, 
                Renter = renter, 
                StartDate = DateTime.UtcNow.AddDays(1), 
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = BoardGames.Data.Enums.RequestStatus.Open
            };

            dbContext.Users.AddRange(owner, renter);
            dbContext.Games.Add(game);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.ApproveRequest(request.Id, owner.Id);

            // Assert
            Assert.True(result.IsSuccess);
            
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            
            // Transaction atomicity verifies that Request is deleted
            var requestExists = finalDb.Requests.Any(r => r.Id == request.Id);
            Assert.False(requestExists, "Request should be deleted upon atomic approval.");

            // And Rental is created
            var rentalCreated = finalDb.Rentals.Any(r => r.Game != null && r.Game.Id == game.Id && r.Owner != null && r.Owner.Id == owner.Id && r.Client != null && r.Client.Id == renter.Id);
            Assert.True(rentalCreated, "Rental should be atomically created upon request approval.");
        }

        [Fact]
        public async Task ApproveRequest_FailsRoleSecurityValidation_WhenRenterAttemptsToApprove()
        {
            // Arrange
            using var scope = _fixture.Factory.Services.CreateScope();
            var (service, dbContext) = await CreateTestSubject(scope);

            var random = new Random();
            var ownerIdStr = Guid.NewGuid().ToString("N");
            var renterIdStr = Guid.NewGuid().ToString("N");
            var owner = new User { Id = Guid.NewGuid(), DisplayName = "OwnerSec", PamUserId = random.Next(10000, 99999), Username = "ownerSec" + ownerIdStr, Email = ownerIdStr + "@test.com", PasswordHash = "hash", Country = "TestCountry", City = "TestCity" };
            var renter = new User { Id = Guid.NewGuid(), DisplayName = "RenterSec", PamUserId = random.Next(10000, 99999), Username = "renterSec" + renterIdStr, Email = renterIdStr + "@test.com", PasswordHash = "hash", Country = "TestCountry", City = "TestCity" };
            var game = new Game { Name = "Security Game", Owner = owner, IsActive = true };
            
            var request = new Request 
            { 
                Game = game, 
                Owner = owner, 
                Renter = renter, 
                StartDate = DateTime.UtcNow.AddDays(1), 
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = BoardGames.Data.Enums.RequestStatus.Open
            };

            dbContext.Users.AddRange(owner, renter);
            dbContext.Games.Add(game);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();

            // Act: RENTER attempts to approve the request!
            var result = await service.ApproveRequest(request.Id, renter.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ApproveRequestError.Unauthorized, result.Error);
        }

        [Fact]
        public async Task ApproveRequest_RollsBackTransaction_OnUnexpectedFailure()
        {
            // Arrange
            using var scope = _fixture.Factory.Services.CreateScope();
            var (service, dbContext) = await CreateTestSubject(scope);

            // Force a constraint failure inside the transaction
            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE rentals WITH NOCHECK ADD CONSTRAINT CK_Rentals_Fail CHECK (1 = 0);");

            var random = new Random();
            var ownerIdStr = Guid.NewGuid().ToString("N");
            var renterIdStr = Guid.NewGuid().ToString("N");
            var owner = new User { Id = Guid.NewGuid(), DisplayName = "OwnerFail", PamUserId = random.Next(10000, 99999), Username = "ownerFail" + ownerIdStr, Email = ownerIdStr + "@test.com", PasswordHash = "hash", Country = "TestCountry", City = "TestCity" };
            var renter = new User { Id = Guid.NewGuid(), DisplayName = "RenterFail", PamUserId = random.Next(10000, 99999), Username = "renterFail" + renterIdStr, Email = renterIdStr + "@test.com", PasswordHash = "hash", Country = "TestCountry", City = "TestCity" };
            var game = new Game { Name = "Fail Game", Owner = owner, IsActive = true };
            
            var request = new Request 
            { 
                Game = game, 
                Owner = owner, 
                Renter = renter, 
                StartDate = new DateTime(1999, 1, 1), // This will violate the CK_Rentals_NoPastDates constraint during Rental Insert!
                EndDate = new DateTime(1999, 1, 5),
                Status = BoardGames.Data.Enums.RequestStatus.Open
            };

            dbContext.Users.AddRange(owner, renter);
            dbContext.Games.Add(game);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.ApproveRequest(request.Id, owner.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ApproveRequestError.TransactionFailed, result.Error);

            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            
            // Transaction MUST roll back: Request must still exist, Rental must NOT exist
            var requestStillExists = finalDb.Requests.Any(r => r.Id == request.Id);
            Assert.True(requestStillExists, "Request should still exist because transaction rolled back.");

            var rentalCreated = finalDb.Rentals.Any(r => r.Game != null && r.Game.Id == game.Id && r.Owner != null && r.Owner.Id == owner.Id && r.Client != null && r.Client.Id == renter.Id);
            Assert.False(rentalCreated, "Rental should not exist due to transaction rollback.");

            // Cleanup constraint
            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE rentals DROP CONSTRAINT CK_Rentals_Fail;");
        }
    }
}
