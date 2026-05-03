using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Api.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
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

            notification.Recipient = ResolveAccount(dbContext, notification.Recipient);
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
                existing.Recipient = ResolveAccount(dbContext, updated.Recipient);
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
            return NotificationsWithRecipient(dbContext)
                .Where(notification => notification.Recipient != null && notification.Recipient.Id == accountId)
                .ToImmutableList();
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var toDelete = dbContext.Notifications
                .Where(notification => notification.RelatedRequest != null && notification.RelatedRequest.Id == relatedRequestId)
                .ToList();

            dbContext.Notifications.RemoveRange(toDelete);
            dbContext.SaveChanges();
        }

        private static Account? ResolveAccount(AppDbContext dbContext, Account? account)
        {
            if (account == null)
            {
                return null;
            }

            var cached = dbContext.Accounts.Local.FirstOrDefault(cachedAccount => cachedAccount.Id == account.Id);
            if (cached != null)
            {
                return cached;
            }

            if (dbContext.Entry(account).State == EntityState.Detached)
            {
                dbContext.Attach(account);
            }

            return account;
        }

        private static Request? ResolveRequest(AppDbContext dbContext, Request? request)
        {
            if (request == null)
            {
                return null;
            }

            var cached = dbContext.Requests.Local.FirstOrDefault(cachedRequest => cachedRequest.Id == request.Id);
            if (cached != null)
            {
                return cached;
            }

            if (dbContext.Entry(request).State == EntityState.Detached)
            {
                dbContext.Attach(request);
            }

            return request;
        }
    }
}
