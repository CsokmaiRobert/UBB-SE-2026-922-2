using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Models;

namespace BoardRentAndProperty.Api.Repositories
{
    public interface INotificationRepository
    {
        ImmutableList<Notification> GetAll();
        void Add(Notification notification);
        Notification Delete(int id);
        void Update(int id, Notification updated);
        Notification Get(int id);
        ImmutableList<Notification> GetNotificationsByUser(Guid accountId);
        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}
