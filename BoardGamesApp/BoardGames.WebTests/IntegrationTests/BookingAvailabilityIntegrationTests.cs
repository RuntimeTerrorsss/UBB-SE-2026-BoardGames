#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1633 // The file header is missing
#pragma warning disable SA1518 // File is required to end with a single newline character
#pragma warning disable SA1028 // Code should not contain trailing whitespace
#pragma warning disable SA1513 // Closing brace should be followed by blank line
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoardGames.WebTests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class BookingAvailabilityIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public BookingAvailabilityIntegrationTests(SharedDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CheckAvailability_WithOverlappingRental_ReturnsFalse()
        {
            // Arrange
            // Test Scenario Focus: EF Core query logic for date overlap computation
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = dbFactory.CreateDbContext();
            
            var owner = await db.Users.FirstAsync(u => u.PamUserId == 200);
            var renter = await db.Users.FirstAsync(u => u.PamUserId == 201);

            // Create a unique game for this test to avoid crosstalk
            var game = new Game 
            { 
                Name = "Carcassonne_AvailabilityTest", 
                MinimumPlayerNumber = 2, 
                MaximumPlayerNumber = 5, 
                PricePerDay = 10, 
                IsActive = true, 
                Description = "Carcassonne", 
                OwnerId = owner.PamUserId 
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();

            var baseDate = DateTime.UtcNow.AddDays(10).Date;

            var rental = new Rental
            {
                Game = game,
                Client = renter,
                Owner = owner,
                StartDate = baseDate,
                EndDate = baseDate.AddDays(5)
            };
            db.Rentals.Add(rental);
            await db.SaveChangesAsync();

            var requestService = scope.ServiceProvider.GetRequiredService<IRequestService>();

            // Act & Assert
            // 1. Exact overlap
            bool isAvailable1 = requestService.CheckAvailability(game.Id, baseDate, baseDate.AddDays(5));
            Assert.False(isAvailable1, "Should be unavailable due to exact rental overlap");

            // 2. Partial overlap (start date inside rental)
            bool isAvailable2 = requestService.CheckAvailability(game.Id, baseDate.AddDays(2), baseDate.AddDays(7));
            Assert.False(isAvailable2, "Should be unavailable due to partial overlap (starts during rental)");

            // 3. Partial overlap (end date inside rental)
            bool isAvailable3 = requestService.CheckAvailability(game.Id, baseDate.AddDays(-2), baseDate.AddDays(2));
            Assert.False(isAvailable3, "Should be unavailable due to partial overlap (ends during rental)");

            // 4. No overlap (before)
            bool isAvailable4 = requestService.CheckAvailability(game.Id, baseDate.AddDays(-5), baseDate.AddDays(-1));
            Assert.True(isAvailable4, "Should be available because it ends before rental starts");

            // 5. No overlap (after)
            bool isAvailable5 = requestService.CheckAvailability(game.Id, baseDate.AddDays(6), baseDate.AddDays(10));
            Assert.True(isAvailable5, "Should be available because it starts after rental ends");
        }

        [Fact]
        public async Task OfferGame_SimultaneousApprovals_FailGracefullyOnTransactionCommits()
        {
            // Arrange
            // Test Scenario Focus: Concurrency validation to avoid double-bookings via transactions
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = dbFactory.CreateDbContext();
            
            var owner = await db.Users.FirstAsync(u => u.PamUserId == 200);
            var renter = await db.Users.FirstAsync(u => u.PamUserId == 201);

            var game = new Game 
            { 
                Name = "7Wonders_ConcurrencyTest", 
                MinimumPlayerNumber = 3, 
                MaximumPlayerNumber = 7, 
                PricePerDay = 12, 
                IsActive = true, 
                Description = "7 Wonders", 
                OwnerId = owner.PamUserId 
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();

            var baseDate = DateTime.UtcNow.AddDays(20).Date;

            // We create 2 identical requests for the exact same dates by the same renter 
            // (or different renters, doesn't matter for the owner's conflict resolution).
            var request1 = new Request
            {
                Game = game,
                Renter = renter,
                Owner = owner,
                StartDate = baseDate,
                EndDate = baseDate.AddDays(3),
                Status = BoardGames.Data.Enums.RequestStatus.Open
            };
            
            var request2 = new Request
            {
                Game = game,
                Renter = renter,
                Owner = owner,
                StartDate = baseDate,
                EndDate = baseDate.AddDays(3),
                Status = BoardGames.Data.Enums.RequestStatus.Open
            };

            db.Requests.AddRange(request1, request2);
            await db.SaveChangesAsync();

            var requestService1 = scope.ServiceProvider.GetRequiredService<IRequestService>();
            
            // To simulate a true concurrent race condition on the DB, we need a second scope/service instance
            using var scope2 = _fixture.Factory.Services.CreateScope();
            var requestService2 = scope2.ServiceProvider.GetRequiredService<IRequestService>();

            // Act
            // Both requests are overlapping. If the owner tries to approve both simultaneously:
            var task1 = requestService1.OfferGame(request1.Id, owner.Id);
            var task2 = requestService2.OfferGame(request2.Id, owner.Id);

            var results = await Task.WhenAll(task1, task2);

            // Assert
            // One should succeed, one should fail gracefully due to transaction or concurrency conflicts
            var successes = results.Count(r => r.IsSuccess);
            var failures = results.Count(r => !r.IsSuccess);

            Assert.Equal(1, successes);
            Assert.Equal(1, failures);

            // Verify the DB state: only ONE rental should exist
            using var assertDb = dbFactory.CreateDbContext();
            var rentals = await assertDb.Rentals.Where(r => r.Game!.Id == game.Id).ToListAsync();
            Assert.Single(rentals);

            // Verify requests: the approved one should be deleted, and the conflicting one should also be deleted by ApproveAtomically
            var remainingRequests = await assertDb.Requests.Where(r => r.Game!.Id == game.Id).ToListAsync();
            Assert.Empty(remainingRequests);
        }
    }
}
