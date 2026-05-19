using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface IDesktopNotificationService : IObservable<NotificationDTO>
    {
        Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateNotificationByIdentifierAsync(
            int notificationId,
            NotificationDTO updatedNotification,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> SendNotificationToUserAsync(
            Guid recipientAccountId,
            NotificationDTO notification,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(
            int relatedRequestId,
            CancellationToken cancellationToken = default);

        void SubscribeToServer(Guid accountId);

        void StartListening();

        void StopListening();
    }
}
