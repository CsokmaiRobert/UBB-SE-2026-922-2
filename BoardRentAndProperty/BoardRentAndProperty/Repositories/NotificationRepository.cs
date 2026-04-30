using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Data;
using BoardRentAndProperty.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext dbContext;

        public NotificationRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private IQueryable<Notification> NotificationsWithRecipient =>
            dbContext.Notifications.Include(notification => notification.Recipient);

        public ImmutableList<Notification> GetAll()
        {
            return NotificationsWithRecipient.ToImmutableList();
        }

        public void Add(Notification notification)
        {
            notification.Recipient = ResolveAccount(notification.Recipient);
            if (notification.RelatedRequest != null)
            {
                notification.RelatedRequest = ResolveRequest(notification.RelatedRequest);
            }
            dbContext.Notifications.Add(notification);
            dbContext.SaveChanges();
        }

        public Notification Delete(int id)
        {
            var notification = NotificationsWithRecipient.FirstOrDefault(notification => notification.Id == id);
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
            var existing = NotificationsWithRecipient.FirstOrDefault(notification => notification.Id == id);
            if (existing == null)
            {
                return;
            }
            if (updated.Recipient != null)
            {
                existing.Recipient = ResolveAccount(updated.Recipient);
            }
            existing.Timestamp = updated.Timestamp;
            existing.Title = updated.Title;
            existing.Body = updated.Body;
            existing.Type = updated.Type;
            existing.RelatedRequest = updated.RelatedRequest != null
                ? ResolveRequest(updated.RelatedRequest)
                : null;
            dbContext.SaveChanges();
        }

        public Notification Get(int id)
        {
            var notification = NotificationsWithRecipient.FirstOrDefault(notification => notification.Id == id);
            if (notification == null)
            {
                throw new KeyNotFoundException();
            }
            return notification;
        }

        public ImmutableList<Notification> GetNotificationsByUser(Guid accountId)
        {
            return NotificationsWithRecipient
                .Where(notification => notification.Recipient.Id == accountId)
                .ToImmutableList();
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            var toDelete = dbContext.Notifications
                .Where(notification => notification.RelatedRequest != null && notification.RelatedRequest.Id == relatedRequestId)
                .ToList();
            dbContext.Notifications.RemoveRange(toDelete);
            dbContext.SaveChanges();
        }

        private Account ResolveAccount(Account? account)
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

        private Request ResolveRequest(Request? request)
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
