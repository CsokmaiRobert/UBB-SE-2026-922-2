using System.Collections.Immutable;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        ImmutableList<Notification> GetNotificationsByUser(int userId);

        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}