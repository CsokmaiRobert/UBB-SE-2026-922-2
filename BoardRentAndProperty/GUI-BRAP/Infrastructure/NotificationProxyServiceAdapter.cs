using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;

namespace GUI_BRAP.Infrastructure
{
    public sealed class NotificationProxyServiceAdapter : INotificationProxyService
    {
        private readonly INotificationService notificationService;

        public NotificationProxyServiceAdapter(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public async Task<IReadOnlyList<NotificationDTO>> GetNotificationsForUserAsync(Guid accountId)
            => (await this.notificationService.GetNotificationsForUserAsync(accountId)).ThrowIfFailed();

        public async Task DeleteNotificationAsync(int notificationId)
            => (await this.notificationService.DeleteNotificationByIdentifierAsync(notificationId)).ThrowIfFailed();
    }
}
