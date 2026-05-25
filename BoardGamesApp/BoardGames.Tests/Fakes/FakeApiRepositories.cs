using BoardGames.Data.Repositories;
using BoardGames.Data.Models;
using BoardGames.Data.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeAccountRepository : IAccountRepository
    {
        public Dictionary<Guid, Account?> AccountsById { get; } = new Dictionary<Guid, Account?>();
        public Dictionary<string, Account?> AccountsByUsername { get; } = new Dictionary<string, Account?>();
        public Dictionary<string, Account?> AccountsByEmail { get; } = new Dictionary<string, Account?>();
        public List<Account> Accounts { get; set; } = new List<Account>();
        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int AddRoleCallCount { get; private set; }
        public Account? LastAddedAccount { get; private set; }
        public Account? LastUpdatedAccount { get; private set; }
        public Guid LastRoleAccountId { get; private set; }
        public string LastRoleName { get; private set; } = string.Empty;

        public Task<Account?> GetByIdAsync(Guid id) =>
            Task.FromResult(AccountsById.TryGetValue(id, out var account) ? account : null);

        public Task<Account?> GetByUsernameAsync(string username) =>
            Task.FromResult(AccountsByUsername.TryGetValue(username, out var account) ? account : null);

        public Task<Account?> GetByEmailAsync(string email) =>
            Task.FromResult(AccountsByEmail.TryGetValue(email, out var account) ? account : null);

        public Task<List<Account>> GetAllAsync(int page, int pageSize) =>
            Task.FromResult(Accounts);

        public Task AddAsync(Account account)
        {
            AddCallCount++;
            LastAddedAccount = account;
            Accounts.Add(account);
            AccountsById[account.Id] = account;
            if (!string.IsNullOrEmpty(account.Username))
            {
                AccountsByUsername[account.Username] = account;
            }

            if (!string.IsNullOrEmpty(account.Email))
            {
                AccountsByEmail[account.Email] = account;
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Account account)
        {
            UpdateCallCount++;
            LastUpdatedAccount = account;
            AccountsById[account.Id] = account;
            return Task.CompletedTask;
        }

        public Task AddRoleAsync(Guid accountId, string roleName)
        {
            AddRoleCallCount++;
            LastRoleAccountId = accountId;
            LastRoleName = roleName;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeFailedLoginRepository : IFailedLoginRepository
    {
        public Dictionary<Guid, FailedLoginAttempt?> FailedLoginAttempts { get; } =
            new Dictionary<Guid, FailedLoginAttempt?>();
        public int IncrementCallCount { get; private set; }
        public int ResetCallCount { get; private set; }
        public Guid LastAccountId { get; private set; }

        public Task<FailedLoginAttempt?> GetByAccountIdAsync(Guid accountId) =>
            Task.FromResult(FailedLoginAttempts.TryGetValue(accountId, out var attempt) ? attempt : null);

        public Task IncrementAsync(Guid accountId)
        {
            IncrementCallCount++;
            LastAccountId = accountId;
            return Task.CompletedTask;
        }

        public Task ResetAsync(Guid accountId)
        {
            ResetCallCount++;
            LastAccountId = accountId;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeGameRepository : IGameRepository
    {
        public ImmutableList<Game> Games { get; set; } = ImmutableList<Game>.Empty;
        public Dictionary<int, Game> GamesById { get; } = new Dictionary<int, Game>();
        public ImmutableList<Game> GamesByOwner { get; set; } = ImmutableList<Game>.Empty;
        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int DeleteCallCount { get; private set; }
        public Game? LastAddedGame { get; private set; }
        public Game? LastUpdatedGame { get; private set; }
        public int LastUpdatedGameId { get; private set; }
        public int LastDeletedGameId { get; private set; }

        public ImmutableList<Game> GetAll() => Games;

        public void Add(Game game)
        {
            AddCallCount++;
            LastAddedGame = game;
        }

        public Game Delete(int id)
        {
            DeleteCallCount++;
            LastDeletedGameId = id;
            return GamesById.TryGetValue(id, out var game) ? game : new Game { Id = id };
        }

        public void Update(int id, Game updated)
        {
            UpdateCallCount++;
            LastUpdatedGameId = id;
            LastUpdatedGame = updated;
        }

        public Game Get(int id) => GamesById.TryGetValue(id, out var game) ? game : new Game { Id = id };

        public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId) => GamesByOwner;
    }

    internal sealed class FakeRentalRepository : IRentalRepository
    {
        public ImmutableList<Rental> Rentals { get; set; } = ImmutableList<Rental>.Empty;
        public ImmutableList<Rental> RentalsByOwner { get; set; } = ImmutableList<Rental>.Empty;
        public ImmutableList<Rental> RentalsByRenter { get; set; } = ImmutableList<Rental>.Empty;
        public ImmutableList<Rental> RentalsByGame { get; set; } = ImmutableList<Rental>.Empty;
        public Dictionary<int, ImmutableList<Rental>> RentalsByGameId { get; } =
            new Dictionary<int, ImmutableList<Rental>>();
        public Dictionary<int, Rental> RentalsById { get; } = new Dictionary<int, Rental>();
        public int AddConfirmedCallCount { get; private set; }
        public int AddCallCount { get; private set; }
        public int DeleteCallCount { get; private set; }
        public Rental? LastConfirmedRental { get; private set; }

        public ImmutableList<Rental> GetAll() => Rentals;

        public void Add(Rental rental)
        {
            AddCallCount++;
        }

        public Rental Delete(int id)
        {
            DeleteCallCount++;
            return RentalsById.TryGetValue(id, out var rental) ? rental : new Rental { Id = id };
        }

        public void Update(int id, Rental updated)
        {
        }

        public Rental Get(int id) => RentalsById.TryGetValue(id, out var rental) ? rental : new Rental { Id = id };

        public void AddConfirmed(Rental confirmedRental)
        {
            AddConfirmedCallCount++;
            LastConfirmedRental = confirmedRental;
        }

        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId) => RentalsByOwner;

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId) => RentalsByRenter;

        public ImmutableList<Rental> GetRentalsByGame(int gameId) =>
            RentalsByGameId.TryGetValue(gameId, out var rentals) ? rentals : RentalsByGame;
    }

    internal sealed class FakeRequestRepository : IRequestRepository
    {
        public ImmutableList<Request> Requests { get; set; } = ImmutableList<Request>.Empty;
        public ImmutableList<Request> RequestsByOwner { get; set; } = ImmutableList<Request>.Empty;
        public ImmutableList<Request> RequestsByRenter { get; set; } = ImmutableList<Request>.Empty;
        public ImmutableList<Request> RequestsByGame { get; set; } = ImmutableList<Request>.Empty;
        public ImmutableList<Request> OverlappingRequests { get; set; } = ImmutableList<Request>.Empty;
        public Dictionary<int, Request> RequestsById { get; } = new Dictionary<int, Request>();
        public int AddCallCount { get; private set; }
        public int DeleteCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int UpdateStatusCallCount { get; private set; }
        public int ApproveAtomicallyResult { get; set; } = 1;
        public Request? LastAddedRequest { get; private set; }
        public int LastDeletedRequestId { get; private set; }

        public ImmutableList<Request> GetAll() => Requests;

        public void Add(Request request)
        {
            AddCallCount++;
            LastAddedRequest = request;
            RequestsById[request.Id] = request;
        }

        public Request Delete(int id)
        {
            DeleteCallCount++;
            LastDeletedRequestId = id;
            return RequestsById.TryGetValue(id, out var request) ? request : new Request { Id = id };
        }

        public void Update(int id, Request updated)
        {
            UpdateCallCount++;
        }

        public Request Get(int id)
        {
            if (RequestsById.TryGetValue(id, out var request))
            {
                return request;
            }

            throw new KeyNotFoundException();
        }

        public void UpdateStatus(int requestId, RequestStatus status, Guid? offeringAccountId)
        {
            UpdateStatusCallCount++;
        }

        public ImmutableList<Request> GetRequestsByOwner(Guid ownerAccountId) => RequestsByOwner;

        public ImmutableList<Request> GetRequestsByRenter(Guid renterAccountId) => RequestsByRenter;

        public ImmutableList<Request> GetRequestsByGame(int gameId) => RequestsByGame;

        public ImmutableList<Request> GetOverlappingRequests(
            int gameId,
            int excludeRequestId,
            DateTime bufferedStartDate,
            DateTime bufferedEndDate) => OverlappingRequests;

        public int ApproveAtomically(Request approvedRequest, ImmutableList<Request> overlappingRequests) =>
            ApproveAtomicallyResult;
    }

    internal sealed class FakeNotificationRepository : INotificationRepository
    {
        public ImmutableList<Notification> Notifications { get; set; } = ImmutableList<Notification>.Empty;
        public ImmutableList<Notification> NotificationsByUser { get; set; } = ImmutableList<Notification>.Empty;
        public Dictionary<int, Notification> NotificationsById { get; } = new Dictionary<int, Notification>();
        public int AddCallCount { get; private set; }
        public int DeleteLinkedCallCount { get; private set; }
        public int LastLinkedRequestId { get; private set; }
        public Notification? LastAddedNotification { get; private set; }

        public ImmutableList<Notification> GetAll() => Notifications;

        public void Add(Notification notification)
        {
            AddCallCount++;
            LastAddedNotification = notification;
        }

        public Notification Delete(int id) =>
            NotificationsById.TryGetValue(id, out var notification) ? notification : new Notification { Id = id };

        public void Update(int id, Notification updated)
        {
        }

        public Notification Get(int id) =>
            NotificationsById.TryGetValue(id, out var notification) ? notification : new Notification { Id = id };

        public ImmutableList<Notification> GetNotificationsByUser(Guid accountId) => NotificationsByUser;

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            DeleteLinkedCallCount++;
            LastLinkedRequestId = relatedRequestId;
        }
    }

    internal sealed class FakeAvatarStorageService : IAvatarStorageService
    {
        public string SavedPath { get; set; } = string.Empty;
        public int DeleteCallCount { get; private set; }
        public string LastDeletedPath { get; private set; } = string.Empty;

        public Task<string> SaveAsync(Guid accountId, Stream content, string fileExtension) =>
            Task.FromResult(SavedPath);

        public void Delete(string relativeUrl)
        {
            DeleteCallCount++;
            LastDeletedPath = relativeUrl;
        }
    }
}
