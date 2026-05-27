// <copyright file="NotificationRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private const string RelatedRequestIdShadowProperty = "related_request_id";

        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public NotificationRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Notification> NotificationsWithRecipient(AppDbContext dbContext) =>
            dbContext.Notifications.Include(notification => notification.Recipient);

        public ImmutableList<Notification> GetAll()
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return NotificationsWithRecipient(dbContext).ToImmutableList();
        }

        public void Add(Notification notification)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            notification.Recipient = ResolveUser(dbContext, notification.Recipient);
            if (notification.RelatedRequest != null)
            {
                notification.RelatedRequest = ResolveRequest(dbContext, notification.RelatedRequest);
            }

            dbContext.Notifications.Add(notification);
            dbContext.SaveChanges();
        }

        public Notification Delete(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var notification = NotificationsWithRecipient(dbContext).FirstOrDefault(repositoryNotification => repositoryNotification.Id == id);
            if (notification == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Notifications.Remove(notification);
            dbContext.SaveChanges();
            return notification;
        }

        public void Update(int id, Notification updated)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = NotificationsWithRecipient(dbContext).FirstOrDefault(notification => notification.Id == id);
            if (existing == null)
            {
                throw new KeyNotFoundException();
            }

            if (updated.Recipient != null)
            {
                existing.Recipient = ResolveUser(dbContext, updated.Recipient);
            }

            existing.Timestamp = updated.Timestamp;
            existing.Title = updated.Title;
            existing.Body = updated.Body;
            existing.Type = updated.Type;
            existing.RelatedRequest = updated.RelatedRequest != null
                ? ResolveRequest(dbContext, updated.RelatedRequest)
                : null;

            dbContext.SaveChanges();
        }

        public Notification Get(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var notification = NotificationsWithRecipient(dbContext).FirstOrDefault(repositoryNotification => repositoryNotification.Id == id);
            if (notification == null)
            {
                throw new KeyNotFoundException();
            }

            return notification;
        }

        public ImmutableList<Notification> GetNotificationsByUser(Guid accountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == accountId);

            if (user == null || user.PamUserId == 0)
            {
                return ImmutableList<Notification>.Empty;
            }

            return NotificationsWithRecipient(dbContext)
                .Where(notification => notification.Recipient != null && notification.Recipient.PamUserId == user.PamUserId)
                .OrderByDescending(notification => notification.Id)
                .ToImmutableList();
        }

        public (ImmutableList<Notification> Items, int TotalCount) GetPagedNotificationsByUser(Guid accountId, int page, int pageSize)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == accountId);
            if (user == null || user.PamUserId == 0)
            {
                return (ImmutableList<Notification>.Empty, 0);
            }

            var query = NotificationsWithRecipient(dbContext)
                .Where(notification => notification.Recipient != null && notification.Recipient.PamUserId == user.PamUserId);

            int totalCount = query.Count();

            var items = query
                .OrderByDescending(notification => notification.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToImmutableList();

            return (items, totalCount);
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            dbContext.Notifications
                .Where(notification => EF.Property<int?>(notification, RelatedRequestIdShadowProperty) == (int?)relatedRequestId)
                .ExecuteDelete();
        }

        private static User? ResolveUser(AppDbContext dbContext, User? user)
        {
            if (user == null)
            {
                return null;
            }

            return dbContext.Users.Find(user.Id);
        }

        private static Request? ResolveRequest(AppDbContext dbContext, Request? request)
        {
            if (request == null)
            {
                return null;
            }

            return dbContext.Requests.Find(request.Id);
        }
    }
}
