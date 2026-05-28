using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;

namespace BoardGames.Tests.IntegrationTests.Web
{
    public sealed class StubGamesRepository : InterfaceGamesRepository
    {
        public Task<Game?> GetGameById(int gameId) => Task.FromResult<Game?>(new Game { Id = gameId, Name = "Stub Game", OwnerId = 1, PricePerDay = 10m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Stub", IsActive = true });
        public Task<decimal> GetPriceGameById(int gameId) => Task.FromResult(10m);
        public Task<List<Game>> GetAll() => Task.FromResult(new List<Game> { new Game { Id = 1, Name = "Stub Game", OwnerId = 1, PricePerDay = 10m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Stub", IsActive = true } });
        public Task<List<Game>> GetGamesByFilter(FilterCriteria filter) => GetAll();
        public Task<List<Game>> GetGamesForFeedAvailableTonight(int userId) => GetAll();
        public Task<List<Game>> GetRemainingGamesForFeed(int userId) => Task.FromResult(new List<Game>());
        public void AddGame(Game game) => throw new NotImplementedException();
        public Game DeleteGame(int id) => throw new NotImplementedException();
        public void UpdateGame(int id, Game updated) => throw new NotImplementedException();
        public Game GetGame(int id) => new Game { Id = id, Name = "Stub Game", OwnerId = 1, PricePerDay = 10m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Stub", IsActive = true };
        public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId) => ImmutableList<Game>.Empty;
        public void Add(Game entity) => throw new NotImplementedException();
        public Game Delete(int id) => throw new NotImplementedException();
        public void Update(int id, Game entity) => throw new NotImplementedException();
        public Game Get(int id) => new Game { Id = id, Name = "Stub Game", OwnerId = 1, PricePerDay = 10m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Stub", IsActive = true };
    }

    public sealed class StubConversationRepository : IConversationRepository
    {
        public Task<List<Conversation>> GetConversationsForUser(int userId) => Task.FromResult(new List<Conversation>());
        public Task<Conversation> GetConversationById(int conversationId) => Task.FromResult(new Conversation());
        public Task<Conversation> FindOrCreateConversationBetweenUsers(int userId, int otherUserId) => Task.FromResult(new Conversation());
        public Task<Message> HandleNewMessage(Message message) => Task.FromResult(message);
        public Task<Message?> HandleMessageUpdate(Message message) => Task.FromResult<Message?>(message);
        public Task HandleReadReceipt(ReadReceiptDTO dto) => Task.CompletedTask;
        public Task<Message?> FindRentalRequestMessageByRequestId(int requestId) => Task.FromResult<Message?>(null);
        public Task<Message?> CreateCashAgreementMessage(int parentMessageId, int paymentId) => Task.FromResult<Message?>(null);
    }

    public sealed class StubPaymentRepository : IPaymentRepository
    {
        public Task<Payment?> GetPaymentByIdentifierAsync(int id) => Task.FromResult<Payment?>(null);
        public Task<IReadOnlyList<Payment>> GetAllPaymentsAsync() => Task.FromResult<IReadOnlyList<Payment>>(Array.Empty<Payment>());
        public Task<int> AddPaymentAsync(Payment payment) => Task.FromResult(1);
        public Task<Payment?> UpdatePaymentAsync(Payment payment) => Task.FromResult<Payment?>(payment);
        public Task<bool> DeletePaymentAsync(Payment payment) => Task.FromResult(true);
    }

    public sealed class StubRentalRepository : IRentalRepository
    {
        public Task<Rental?> GetById(int rentalId) => Task.FromResult<Rental?>(null);
        public Task<TimeRange?> GetRentalTimeRange(int rentalId) => Task.FromResult<TimeRange?>(null);
        public Task<List<TimeRange>> GetAllOccupiedPeriods() => Task.FromResult(new List<TimeRange>());
        public Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId) => Task.FromResult(new List<TimeRange>());
        public Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId) => Task.FromResult(true);
        public Task AddRental(Rental rental) => Task.CompletedTask;
        public Task<List<Rental>> GetRentalsForUser(int userId) => Task.FromResult(new List<Rental>());
        public Task BookGameWithRentalRequest(int clientId, int gameId, DateTime startDate, DateTime endDate) => Task.CompletedTask;
        public ImmutableList<Rental> GetAll() => ImmutableList<Rental>.Empty;
        public void Add(Rental rental) => throw new NotImplementedException();
        public Rental Delete(int id) => throw new NotImplementedException();
        public void Update(int id, Rental updated) => throw new NotImplementedException();
        public Rental Get(int id) => throw new NotImplementedException();
        public void AddConfirmed(Rental confirmedRental) => throw new NotImplementedException();
        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId) => ImmutableList<Rental>.Empty;
        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId) => ImmutableList<Rental>.Empty;
        public ImmutableList<Rental> GetRentalsByGame(int gameId) => ImmutableList<Rental>.Empty;
    }

    public sealed class StubRepositoryPayment : IRepositoryPayment
    {
        public Task<IReadOnlyList<HistoryPayment>> GetAllPayments() => Task.FromResult<IReadOnlyList<HistoryPayment>>(Array.Empty<HistoryPayment>());
        public Task<HistoryPayment?> GetPaymentById(int id) => Task.FromResult<HistoryPayment?>(null);
    }

    public sealed class StubUserRepository : IUserRepository
    {
        public Task<User?> GetById(int userId) => Task.FromResult<User?>(new User { PamUserId = userId, Username = "stub", DisplayName = "Stub" });
        public Task<User?> GetByUsernameAsync(string username) => Task.FromResult<User?>(new User { PamUserId = 1, Username = username, DisplayName = "Stub" });
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult<User?>(new User { PamUserId = 1, Email = email, Username = "stub", DisplayName = "Stub" });
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(new List<User> { new User { PamUserId = 1, Username = "stub", DisplayName = "Stub" } });
        public Task<bool> AddUserAsync(User user) => Task.FromResult(true);
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> DeleteUserAsync(int userId) => Task.FromResult(true);
        public Task<User?> LoginAsync(string identifier, string password) => Task.FromResult<User?>(new User { PamUserId = 1, Username = "stub", DisplayName = "Stub" });
        public Task<List<User>> GetUsersExcept(int excludedUserId) => Task.FromResult(new List<User>());
        public Task<List<User>> GetUsersByIdsAsync(IEnumerable<int> userIds) => Task.FromResult(new List<User>());
        public Task<User?> GetGameById(int gameId) => Task.FromResult<User?>(new User { PamUserId = 1, Username = "stub", DisplayName = "Stub" });
        public Task<bool> AddRoleAsync(Guid accountId, string roleName) => Task.FromResult(true);
    }
}
