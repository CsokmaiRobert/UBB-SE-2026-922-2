using System;
using System.Collections.Immutable;
using BoardRentAndProperty.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface INotificationService : IObservable<NotificationDTO>
    {
        event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        NotificationConnectionStatus ConnectionStatus { get; }

        NotificationDTO GetNotificationByIdentifier(int notificationId);

        NotificationDTO DeleteNotificationByIdentifier(int notificationId);

        void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto);

        void SendNotificationToUser(int recipientUserId, NotificationDTO notificationDto);

        ImmutableList<NotificationDTO> GetNotificationsForUser(int userId);

        void SubscribeToServer(int targetUserId);

        void StartListening();

        void StopListening();

        void ScheduleUpcomingRentalReminder(int rentalId, int renterUserId, int ownerUserId, string gameName, DateTime rentalStartDate);

        void CancelUpcomingRentalReminder(int rentalId);
    }
}
