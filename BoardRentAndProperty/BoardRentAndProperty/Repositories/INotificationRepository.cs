using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
namespace BoardRentAndProperty.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        ImmutableList<Notification> GetNotificationsByUser(Guid accountId);
        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}
