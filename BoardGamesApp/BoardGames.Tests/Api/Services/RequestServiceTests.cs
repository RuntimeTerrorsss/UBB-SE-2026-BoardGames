using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Api.Mappers;
using BoardGames.Api.Services;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.Api.Services
{
    [TestFixture]
    public sealed class RequestServiceTests
    {
        private FakeRequestRepository requestRepository = null!;
        private FakeRentalRepository rentalRepository = null!;
        private FakeGameRepository gameRepository = null!;
        private FakeApiNotificationService notificationService = null!;
        private FakeConversationApiService conversationService = null!;
        private RequestService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestRepository = new FakeRequestRepository();
            this.rentalRepository = new FakeRentalRepository();
            this.gameRepository = new FakeGameRepository();
            this.notificationService = new FakeApiNotificationService();
            this.conversationService = new FakeConversationApiService();

            this.service = new RequestService(
                this.requestRepository,
                this.rentalRepository,
                this.gameRepository,
                this.notificationService,
                this.conversationService,
                new RequestMapper(new GameMapper(new UserMapper()), new UserMapper()));
        }

        [Test]
        public async Task CreateRequest_WhenRenterIsOwner_ReturnsOwnerCannotRent()
        {
            var ownerId = Guid.NewGuid();
            this.gameRepository.GamesById[10] = new Game
            {
                Id = 10,
                Owner = new User { Id = ownerId, DisplayName = "Owner" },
                IsActive = true,
            };

            var result = await this.service.CreateRequest(
                10,
                ownerId,
                ownerId,
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(4));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.OwnerCannotRent));
        }

        [Test]
        public async Task CreateRequest_WhenDateRangeIsInvalid_ReturnsInvalidDateRange()
        {
            var result = await this.service.CreateRequest(
                10,
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(4),
                DateTime.UtcNow.AddDays(2));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.InvalidDateRange));
        }

        [Test]
        public void CancelRequest_AsRenter_DeletesRequestAndNotifications()
        {
            var renterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var existingRequest = new Request
            {
                Id = 100,
                Renter = new Account { Id = renterId, DisplayName = "Renter" },
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                Game = new Game { Id = 10, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RequestStatus.Open,
            };

            this.requestRepository.RequestsById[100] = existingRequest;

            var result = this.service.CancelRequest(100, renterId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(100));
            Assert.That(this.requestRepository.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.requestRepository.LastDeletedRequestId, Is.EqualTo(100));
            Assert.That(this.notificationService.DeleteLinkedNotificationCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.LastLinkedRequestId, Is.EqualTo(100));
        }

        [Test]
        public async Task CreateRequest_WhenGameDoesNotExist_ReturnsGameDoesNotExist()
        {
            var renterAccountId = Guid.NewGuid();
            var ownerAccountId = Guid.NewGuid();

            var result = await this.service.CreateRequest(
                999,
                renterAccountId,
                ownerAccountId,
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(5));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.GameDoesNotExist));
        }

        [Test]
        public async Task CreateRequest_WhenDatesAreUnavailable_ReturnsDatesUnavailable()
        {
            int gameId = 10;
            var ownerAccountId = Guid.NewGuid();
            var renterAccountId = Guid.NewGuid();

            this.gameRepository.GamesById[gameId] = new Game
            {
                Id = gameId,
                Owner = new User { Id = ownerAccountId },
                IsActive = true,
            };

            var conflictingRental = new Rental
            {
                StartDate = new DateTime(2026, 6, 6),
                EndDate = new DateTime(2026, 6, 8),
            };

            this.rentalRepository.RentalsByGameId[gameId] = ImmutableList.Create(conflictingRental);

            var result = await this.service.CreateRequest(
                gameId,
                renterAccountId,
                ownerAccountId,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 10));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.DatesUnavailable));
        }

        [Test]
        public async Task CreateRequest_ValidRequest_AddsRequestAndSendsNotificationToOwner()
        {
            int gameId = 10;
            var ownerAccountId = Guid.NewGuid();
            var renterAccountId = Guid.NewGuid();

            this.gameRepository.GamesById[gameId] = new Game
            {
                Id = gameId,
                Name = "Chess",
                Owner = new User { Id = ownerAccountId },
                IsActive = true,
            };

            var result = await this.service.CreateRequest(
                gameId,
                renterAccountId,
                ownerAccountId,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 10));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(this.requestRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.SendNotificationCallCount, Is.EqualTo(1));
            Assert.That(this.conversationService.AttachRentalRequestMessageCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ApproveRequest_WhenRequestNotFound_ReturnsNotFound()
        {
            var result = await this.service.ApproveRequest(999, Guid.NewGuid());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ApproveRequestError.NotFound));
        }

        [Test]
        public async Task ApproveRequest_WhenCallerIsNotOwner_ReturnsUnauthorized()
        {
            var ownerAccountId = Guid.NewGuid();
            var differentAccountId = Guid.NewGuid();

            this.requestRepository.RequestsById[1] = new Request
            {
                Id = 1,
                Owner = new Account { Id = ownerAccountId },
                Renter = new Account { Id = Guid.NewGuid() },
                Game = new Game { Id = 10, Name = "Chess" },
                Status = RequestStatus.Open,
            };

            var result = await this.service.ApproveRequest(1, differentAccountId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ApproveRequestError.Unauthorized));
        }

        [Test]
        public async Task ApproveRequest_ValidRequest_SendsNotificationToRenter()
        {
            var ownerAccountId = Guid.NewGuid();
            var renterAccountId = Guid.NewGuid();

            this.requestRepository.RequestsById[1] = new Request
            {
                Id = 1,
                Owner = new Account { Id = ownerAccountId },
                Renter = new Account { Id = renterAccountId },
                Game = new Game { Id = 10, Name = "Chess" },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RequestStatus.Open,
            };

            this.requestRepository.OverlappingRequests = ImmutableList<Request>.Empty;
            this.requestRepository.ApproveAtomicallyResult = 5;

            var result = await this.service.ApproveRequest(1, ownerAccountId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(5));
            Assert.That(this.notificationService.SendNotificationCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DenyRequest_WhenRequestNotFound_ReturnsNotFound()
        {
            var result = await this.service.DenyRequest(999, Guid.NewGuid(), "reason");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(DenyRequestError.NotFound));
        }

        [Test]
        public async Task DenyRequest_WhenCallerIsNotOwner_ReturnsUnauthorized()
        {
            var ownerAccountId = Guid.NewGuid();
            var differentAccountId = Guid.NewGuid();

            this.requestRepository.RequestsById[1] = new Request
            {
                Id = 1,
                Owner = new Account { Id = ownerAccountId },
                Renter = new Account { Id = Guid.NewGuid() },
                Game = new Game { Id = 10, Name = "Chess" },
                Status = RequestStatus.Open,
            };

            var result = await this.service.DenyRequest(1, differentAccountId, "reason");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(DenyRequestError.Unauthorized));
        }

        [Test]
        public async Task DenyRequest_ValidRequest_DeletesRequestAndNotifiesRenter()
        {
            var ownerAccountId = Guid.NewGuid();
            var renterAccountId = Guid.NewGuid();

            this.requestRepository.RequestsById[1] = new Request
            {
                Id = 1,
                Owner = new Account { Id = ownerAccountId },
                Renter = new Account { Id = renterAccountId },
                Game = new Game { Id = 10, Name = "Chess" },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RequestStatus.Open,
            };

            var result = await this.service.DenyRequest(1, ownerAccountId, "Not available");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(this.requestRepository.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.DeleteLinkedNotificationCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.SendNotificationCallCount, Is.EqualTo(1));
        }

        [Test]
        public void CancelRequest_WhenRequestNotFound_ReturnsNotFound()
        {
            var result = this.service.CancelRequest(999, Guid.NewGuid());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CancelRequestError.NotFound));
        }

        [Test]
        public void CancelRequest_WhenCallerIsNotRenter_ReturnsUnauthorized()
        {
            var renterAccountId = Guid.NewGuid();
            var differentAccountId = Guid.NewGuid();

            this.requestRepository.RequestsById[1] = new Request
            {
                Id = 1,
                Renter = new Account { Id = renterAccountId },
                Owner = new Account { Id = Guid.NewGuid() },
                Game = new Game { Id = 10, Name = "Chess" },
                Status = RequestStatus.Open,
            };

            var result = this.service.CancelRequest(1, differentAccountId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CancelRequestError.Unauthorized));
        }

        [Test]
        public void CheckAvailability_WhenGameIsNotActive_ReturnsFalse()
        {
            int gameId = 10;
            this.gameRepository.GamesById[gameId] = new Game { Id = gameId, IsActive = false };

            bool isAvailable = this.service.CheckAvailability(
                gameId,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 10));

            Assert.That(isAvailable, Is.False);
        }

        [Test]
        public void CheckAvailability_WhenRentalConflictExists_ReturnsFalse()
        {
            int gameId = 10;
            this.gameRepository.GamesById[gameId] = new Game { Id = gameId, IsActive = true };

            var conflictingRental = new Rental
            {
                StartDate = new DateTime(2026, 6, 6),
                EndDate = new DateTime(2026, 6, 8),
            };

            this.rentalRepository.RentalsByGameId[gameId] = ImmutableList.Create(conflictingRental);

            bool isAvailable = this.service.CheckAvailability(
                gameId,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 10));

            Assert.That(isAvailable, Is.False);
        }

        [Test]
        public void OnGameDeactivated_CancelsOpenRequestsAndNotifiesEachRenter()
        {
            int gameId = 10;
            var firstRenterAccountId = Guid.NewGuid();
            var secondRenterAccountId = Guid.NewGuid();

            this.requestRepository.RequestsByGame = ImmutableList.Create(
                new Request
                {
                    Id = 1,
                    Renter = new Account { Id = firstRenterAccountId },
                    Game = new Game { Id = gameId, Name = "Chess" },
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    Status = RequestStatus.Open,
                },
                new Request
                {
                    Id = 2,
                    Renter = new Account { Id = secondRenterAccountId },
                    Game = new Game { Id = gameId, Name = "Chess" },
                    StartDate = DateTime.UtcNow.AddDays(6),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    Status = RequestStatus.Open,
                });

            this.service.OnGameDeactivated(gameId);

            Assert.That(this.requestRepository.DeleteCallCount, Is.EqualTo(2));
            Assert.That(this.notificationService.DeleteLinkedNotificationCallCount, Is.EqualTo(2));
            Assert.That(this.notificationService.SendNotificationCallCount, Is.EqualTo(2));
        }
    }
}
