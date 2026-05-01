using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Services
{
    public interface INotificationService
    {
        ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId);
        NotificationDTO GetNotificationByIdentifier(int notificationId);
        NotificationDTO DeleteNotificationByIdentifier(int notificationId);
        void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto);
        void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto);
        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}
