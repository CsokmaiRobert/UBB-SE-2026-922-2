using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;

namespace GUI_BRAP.ProxyServices
{
    public class NotificationProxyService : INotificationProxyService
    {
        private readonly HttpClient httpClient;

        public NotificationProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClient = httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
        }

        public async Task<IReadOnlyList<NotificationDTO>> GetNotificationsForUserAsync(Guid accountId)
        {
            var response = await this.httpClient.GetAsync($"api/notifications/user/{accountId}");
            await HttpResponseEnsurer.EnsureSuccessAsync(response);

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<NotificationDTO>>()
                ?? Array.Empty<NotificationDTO>();
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var response = await this.httpClient.DeleteAsync($"api/notifications/{notificationId}");

            await HttpResponseEnsurer.EnsureSuccessAsync(response);
        }
    }
}