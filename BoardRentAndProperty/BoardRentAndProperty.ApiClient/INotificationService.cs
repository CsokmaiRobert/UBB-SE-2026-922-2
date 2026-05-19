using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public interface INotificationService
    {
        Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(int notificationId, CancellationToken cancellationToken = default);

        Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(int notificationId, CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateNotificationByIdentifierAsync(int notificationId, NotificationDTO notification, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(int relatedRequestId, CancellationToken cancellationToken = default);

        Task<ServiceResult> PersistNotificationAsync(NotificationDTO notification, CancellationToken cancellationToken = default);
    }
}
