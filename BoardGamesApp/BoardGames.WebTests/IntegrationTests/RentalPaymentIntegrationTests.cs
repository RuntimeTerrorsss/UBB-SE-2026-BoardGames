using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Api.Services;
using BoardGames.Shared.DTO;
using BoardGames.Data.Constants;

namespace BoardGames.WebTests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class RentalPaymentIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public RentalPaymentIntegrationTests(SharedDatabaseFixture fixture)
        {
            this._fixture = fixture;
        }

        private async Task<(RentalPaymentService, AppDbContext, IRepositoryPayment)> CreateTestSubject(IServiceScope scope)
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();

            var rentalRepo = new RentalRepository(dbContext);
            var gamesRepo = new GamesRepository(dbContext); 
            var userRepo = new UserRepository(dbContext);
            var accountRepo = new AccountRepository(dbContextFactory);
            var paymentRepo = new PaymentRepository(dbContext);
            var conversationRepo = new ConversationRepository(dbContext);

            var mockConversationApiService = new Mock<IConversationApiService>();

            var service = new RentalPaymentService(
                rentalRepo,
                gamesRepo,
                userRepo,
                accountRepo,
                paymentRepo,
                conversationRepo,
                mockConversationApiService.Object
            );

            var historyRepo = new RepositoryPayment(dbContext);

            return (service, dbContext, historyRepo);
        }

        [Fact]
        public async Task CompleteCardPaymentAsync_Successful_AddsPaymentAndSyncsHistory()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (service, dbContext, historyRepo) = await this.CreateTestSubject(scope);

            var random = new Random();
            var ownerStr = Guid.NewGuid().ToString("N");
            var renterStr = Guid.NewGuid().ToString("N");

            var owner = new User { Id = Guid.NewGuid(), DisplayName = "PayOwner", PamUserId = random.Next(10000, 99999), Username = "owner" + ownerStr, Email = ownerStr + "@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            var renter = new User { Id = Guid.NewGuid(), DisplayName = "PayRenter", PamUserId = random.Next(10000, 99999), Username = "renter" + renterStr, Email = renterStr + "@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            
            var game = new Game { Name = "Pay Game", Owner = owner, IsActive = true, PricePerDay = 10m };
            
            var rental = new Rental 
            { 
                Game = game, 
                Owner = owner, 
                Client = renter, 
                StartDate = DateTime.UtcNow.AddDays(1), 
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            var conversation = new Conversation();

            dbContext.Users.AddRange(owner, renter);
            dbContext.Games.Add(game);
            dbContext.Rentals.Add(rental);
            dbContext.Conversations.Add(conversation);
            await dbContext.SaveChangesAsync();

            var rentalMessage = new RentalRequestMessage 
            {
                Conversation = conversation,
                Sender = renter,
                Receiver = owner,
                ConversationId = conversation.ConversationId,
                MessageSenderId = renter.PamUserId,
                MessageReceiverId = owner.PamUserId,
                RentalRequestId = rental.Id,
                IsRequestAccepted = true,
                IsRequestResolved = false,
                RequestContent = "Test request",
            };

            dbContext.Messages.Add(rentalMessage);
            await dbContext.SaveChangesAsync();

            var paymentDto = new CompleteRentalCardPaymentDTO
            {
                RequestId = rental.Id,
                RentalId = rental.Id,
                MessageId = rentalMessage.MessageId,
                RenterAccountId = renter.Id,
                CardNumber = "1234567890123",
                CardholderName = "Test Renter",
                ExpiryDate = "12/26",
                CardVerificationValue = "123",
                PaymentMethod = "CARD",
            };

            // Act
            await service.CompleteCardPaymentAsync(paymentDto);

            // Assert
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            
            // Verify payment was saved correctly
            var savedPayment = finalDb.Payments.FirstOrDefault(p => p.RequestId == rental.Id);
            Assert.NotNull(savedPayment);
            Assert.Equal(CardPaymentConstants.SuccessfulPaymentState, savedPayment.PaymentState);
            Assert.Equal(50m, savedPayment.PaidAmount); // 5 days * 10
            Assert.Equal(CardPaymentConstants.CardPaymentMethodName, savedPayment.PaymentMethod);

            // Verify message was updated (finalized)
            var updatedMessage = finalDb.Messages.OfType<RentalRequestMessage>().FirstOrDefault(m => m.MessageId == rentalMessage.MessageId);
            Assert.NotNull(updatedMessage);
            Assert.True(updatedMessage.IsRequestAccepted);
            Assert.True(updatedMessage.IsRequestResolved);

            // Verify history syncs properly via dashboard record DTO
            var history = await historyRepo.GetAllPayments();
            var historyRecord = history.FirstOrDefault(h => h.TransactionIdentifier == savedPayment.TransactionIdentifier);
            Assert.NotNull(historyRecord);
            Assert.Equal("Pay Game", historyRecord.GameName);
            Assert.Equal("PayOwner", historyRecord.OwnerName);
            Assert.Equal("PayRenter", historyRecord.ClientName);
            Assert.Equal(50m, historyRecord.PaidAmount);
        }

        [Fact]
        public async Task CompleteCardPaymentAsync_ZeroValueBooking_HandlesFreeGamesProperly()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (service, dbContext, _) = await this.CreateTestSubject(scope);

            var random = new Random();
            var ownerStr = Guid.NewGuid().ToString("N");
            var renterStr = Guid.NewGuid().ToString("N");

            var owner = new User { Id = Guid.NewGuid(), DisplayName = "ZeroOwner", PamUserId = random.Next(10000, 99999), Username = "owner" + ownerStr, Email = ownerStr + "@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            var renter = new User { Id = Guid.NewGuid(), DisplayName = "ZeroRenter", PamUserId = random.Next(10000, 99999), Username = "renter" + renterStr, Email = renterStr + "@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            
            // Price = 0 represents a zero-value booking
            var game = new Game { Name = "Free Game", Owner = owner, IsActive = true, PricePerDay = 0m };
            
            var rental = new Rental 
            { 
                Game = game, 
                Owner = owner, 
                Client = renter, 
                StartDate = DateTime.UtcNow.AddDays(1), 
                EndDate = DateTime.UtcNow.AddDays(3)
            };

            var conversation = new Conversation();

            dbContext.Users.AddRange(owner, renter);
            dbContext.Games.Add(game);
            dbContext.Rentals.Add(rental);
            dbContext.Conversations.Add(conversation);
            await dbContext.SaveChangesAsync();

            var rentalMessage = new RentalRequestMessage 
            {
                Conversation = conversation,
                Sender = renter,
                Receiver = owner,
                ConversationId = conversation.ConversationId,
                MessageSenderId = renter.PamUserId,
                MessageReceiverId = owner.PamUserId,
                RentalRequestId = rental.Id,
                IsRequestAccepted = true,
                IsRequestResolved = false,
                RequestContent = "Free rental request",
            };

            dbContext.Messages.Add(rentalMessage);
            await dbContext.SaveChangesAsync();

            var paymentDto = new CompleteRentalCardPaymentDTO
            {
                RequestId = rental.Id,
                RentalId = rental.Id,
                MessageId = rentalMessage.MessageId,
                RenterAccountId = renter.Id,
                PaymentMethod = "CASH", // Testing cash payment / zero value combination
            };

            // Act
            await service.CompleteCardPaymentAsync(paymentDto);

            // Assert
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            
            var savedPayment = finalDb.Payments.FirstOrDefault(p => p.RequestId == rental.Id);
            Assert.NotNull(savedPayment);
            Assert.Equal(0m, savedPayment.PaidAmount); // Evaluated zero
            Assert.Equal("CASH", savedPayment.PaymentMethod);
        }
    }
}
