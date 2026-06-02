// <copyright file="RequestRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Data.Enums;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private const string RelatedRequestIdShadowProperty = "related_request_id";

        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public RequestRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Request> RequestsWithNavigations(AppDbContext dbContext) =>
            dbContext.Requests
                .Include(request => request.Game)
                .Include(request => request.Renter)
                .Include(request => request.Owner)
                .Include(request => request.OfferingUser);

        public ImmutableList<Request> GetAll()
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext).ToImmutableList();
        }

        public void Add(Request request)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            request.Game = ResolveGame(dbContext, request.Game);
            request.Renter = ResolveUser(dbContext, request.Renter);
            request.Owner = ResolveUser(dbContext, request.Owner);
            request.OfferingUser = ResolveUser(dbContext, request.OfferingUser);
            dbContext.Requests.Add(request);
            dbContext.SaveChanges();

            var saved = RequestsWithNavigations(dbContext).FirstOrDefault(savedRequest => savedRequest.Id == request.Id);
            if (saved != null)
            {
                request.Game = saved.Game;
                request.Renter = saved.Renter;
                request.Owner = saved.Owner;
                request.OfferingUser = saved.OfferingUser;
            }
        }

        public Request Delete(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var request = RequestsWithNavigations(dbContext).FirstOrDefault(repositoryRequest => repositoryRequest.Id == id);
            if (request == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Requests.Remove(request);
            dbContext.SaveChanges();
            return request;
        }

        public void Update(int id, Request updated)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = RequestsWithNavigations(dbContext).FirstOrDefault(request => request.Id == id);
            if (existing == null)
            {
                return;
            }

            if (updated.Game != null)
            {
                existing.Game = ResolveGame(dbContext, updated.Game);
            }

            if (updated.Renter != null)
            {
                existing.Renter = ResolveUser(dbContext, updated.Renter);
            }

            if (updated.Owner != null)
            {
                existing.Owner = ResolveUser(dbContext, updated.Owner);
            }

            existing.OfferingUser = ResolveUser(dbContext, updated.OfferingUser);
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            existing.Status = updated.Status;
            dbContext.SaveChanges();
        }

        public void UpdateStatus(int requestId, RequestStatus status, Guid? offeringAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = RequestsWithNavigations(dbContext).FirstOrDefault(request => request.Id == requestId);
            if (existing == null)
            {
                return;
            }

            existing.Status = status;
            existing.OfferingUser = FindUserById(dbContext, offeringAccountId);
            dbContext.SaveChanges();
        }

        public Request Get(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var request = RequestsWithNavigations(dbContext).FirstOrDefault(repositoryRequest => repositoryRequest.Id == id);
            if (request == null)
            {
                throw new KeyNotFoundException();
            }

            return request;
        }

        public ImmutableList<Request> GetRequestsByOwner(Guid ownerAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Owner != null && request.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByRenter(Guid renterAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Renter != null && request.Renter.Id == renterAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByGame(int gameId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Game != null && request.Game.Id == gameId)
                .ToImmutableList();
        }

        public ImmutableList<Request> GetOverlappingRequests(int gameId, int excludeRequestId, DateTime bufferedStart, DateTime bufferedEnd)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Game != null && request.Game.Id == gameId
                    && request.Id != excludeRequestId
                    && request.StartDate < bufferedEnd
                    && request.EndDate > bufferedStart)
                .ToImmutableList();
        }

        public int ApproveAtomically(Request approvedRequest, ImmutableList<Request> overlappingRequests)
        {
            using var strategyContext = this.dbContextFactory.CreateDbContext();
            var executionStrategy = strategyContext.Database.CreateExecutionStrategy();
            return executionStrategy.Execute(() =>
            {
                using var dbContext = this.dbContextFactory.CreateDbContext();
                var isInMemory = dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
                using var transaction = isInMemory ? null : dbContext.Database.BeginTransaction();
                try
                {
                    var relatedRequestIdsToDelete = overlappingRequests
                        .Select(conflict => (int?)conflict.Id)
                        .Append((int?)approvedRequest.Id)
                        .Distinct()
                        .ToArray();

                    var notificationsToDelete = dbContext.Notifications
                        .Where(notification => relatedRequestIdsToDelete.Contains(
                            EF.Property<int?>(notification, RelatedRequestIdShadowProperty)))
                        .ToList();
                    dbContext.Notifications.RemoveRange(notificationsToDelete);

                    var newRental = new Rental
                    {
                        Game = ResolveGame(dbContext, approvedRequest.Game),
                        Client = ResolveUser(dbContext, approvedRequest.Renter),
                        Owner = ResolveUser(dbContext, approvedRequest.Owner),
                        StartDate = approvedRequest.StartDate,
                        EndDate = approvedRequest.EndDate,
                    };
                    dbContext.Rentals.Add(newRental);

                    foreach (var conflict in overlappingRequests)
                    {
                        var conflictEntity = dbContext.Requests.FirstOrDefault(request => request.Id == conflict.Id);
                        if (conflictEntity != null)
                        {
                            dbContext.Requests.Remove(conflictEntity);
                        }
                    }

                    var approvedEntity = dbContext.Requests.FirstOrDefault(request => request.Id == approvedRequest.Id);
                    if (approvedEntity != null)
                    {
                        dbContext.Requests.Remove(approvedEntity);
                    }

                    dbContext.SaveChanges();
                    if (transaction != null) transaction.Commit();
                    return newRental.Id;
                }
                catch
                {
                    if (transaction != null) transaction.Rollback();
                    throw;
                }
            });
        }

        private static User? ResolveUser(AppDbContext dbContext, User? user)
        {
            if (user == null)
            {
                return null;
            }

            if (user.PamUserId != 0)
            {
                var trackedByPam = dbContext.Users.Local.FirstOrDefault(cached => cached.PamUserId == user.PamUserId);
                if (trackedByPam != null)
                {
                    return trackedByPam;
                }

                return dbContext.Users.SingleOrDefault(u => u.PamUserId == user.PamUserId);
            }

            var trackedById = dbContext.Users.Local.FirstOrDefault(cachedUser => cachedUser.Id == user.Id);
            if (trackedById != null)
            {
                return trackedById;
            }

            if (user.Id != Guid.Empty)
            {
                return dbContext.Users.Find(user.Id);
            }

            return null;
        }

        private static Game? ResolveGame(AppDbContext dbContext, Game? game)
        {
            if (game == null)
            {
                return null;
            }

            var cached = dbContext.Games.Local.FirstOrDefault(cachedGame => cachedGame.Id == game.Id);
            if (cached != null)
            {
                return cached;
            }

            if (game.Id != 0)
            {
                return dbContext.Games.Find(game.Id);
            }

            return null;
        }

        private static User? FindUserById(AppDbContext dbContext, Guid? accountId)
        {
            if (!accountId.HasValue)
            {
                return null;
            }

            var cached = dbContext.Users.Local.FirstOrDefault(cachedUser => cachedUser.Id == accountId.Value);
            return cached ?? dbContext.Users.Find(accountId.Value);
        }
    }
}
