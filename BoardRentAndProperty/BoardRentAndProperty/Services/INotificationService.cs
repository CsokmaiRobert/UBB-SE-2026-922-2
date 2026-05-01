using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
namespace BoardRentAndProperty.Services
{
    public interface INotificationService : IObservable<NotificationDTO>
    {
        NotificationDTO GetNotificationByIdentifier(int notificationId);
        NotificationDTO DeleteNotificationByIdentifier(int notificationId);
        void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto);
        void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto);
        ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId);
        void SubscribeToServer(Guid accountId);
        void StartListening();
        void StopListening();
        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}
