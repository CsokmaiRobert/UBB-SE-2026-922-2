using System.Collections.Immutable;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Services
{
    public interface INotificationRepository : IRepository<Notification>
    {
        ImmutableList<Notification> GetNotificationsByUser(int userId);

        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}