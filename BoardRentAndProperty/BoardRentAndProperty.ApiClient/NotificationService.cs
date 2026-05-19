using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class NotificationService : ApiServiceBase, INotificationService
    {
        public NotificationService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<NotificationDTO>(
                token => client.GetAsync($"api/notifications/{notificationId}", token),
                (response, token) => ApiResponseReader.ReadJsonAsync<NotificationDTO>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<NotificationDTO>(
                token => client.DeleteAsync($"api/notifications/{notificationId}", token),
                async (response, token) =>
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return await ApiResponseReader.ToFailAsync<NotificationDTO>(response, token);
                    }

                    var parsed = await ApiResponseReader.ReadJsonAsync<NotificationDTO>(response, token);
                    return parsed.Success ? parsed : ServiceResult<NotificationDTO>.Ok(new NotificationDTO { Id = notificationId });
                },
                cancellationToken);
        }

        public Task<ServiceResult> UpdateNotificationByIdentifierAsync(int notificationId, NotificationDTO notification, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync($"api/notifications/{notificationId}", notification, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<NotificationDTO>>(
                token => client.GetAsync($"api/notifications/user/{accountId}", token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<NotificationDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<NotificationDTO>>.Ok(parsed.Data ?? new List<NotificationDTO>())
                        : ServiceResult<IReadOnlyList<NotificationDTO>>.Fail(parsed);
                },
                cancellationToken);
        }

        public Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(int relatedRequestId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.DeleteAsync($"api/notifications/request/{relatedRequestId}", token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult> PersistNotificationAsync(NotificationDTO notification, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync("api/notifications/0", notification, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }
    }
}
