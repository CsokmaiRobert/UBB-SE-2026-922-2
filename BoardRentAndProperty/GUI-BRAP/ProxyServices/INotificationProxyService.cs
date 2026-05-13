using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface INotificationProxyService
    {
        Task<IReadOnlyList<NotificationDTO>> GetNotificationsForUserAsync(Guid accountId);
        Task DeleteNotificationAsync(int notificationId);
    }
}