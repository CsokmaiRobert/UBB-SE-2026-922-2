using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class AdminService : ApiServiceBase, IAdminService
    {
        public AdminService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<AccountProfileDataTransferObject>>(
                token => client.GetAsync($"api/admin/accounts?page={page}&pageSize={pageSize}", token),
                async (response, token) =>
                {
                    var result = await ApiResponseReader.ReadJsonAsync<List<AccountProfileDataTransferObject>>(response, token);
                    if (!result.Success)
                    {
                        return ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>.Fail(result);
                    }

                    var profiles = result.Data ?? new List<AccountProfileDataTransferObject>();
                    foreach (var profile in profiles)
                    {
                        ApiUrlHelper.RebaseAvatarUrl(client.BaseAddress!, profile);
                    }

                    return ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>.Ok(profiles);
                },
                cancellationToken);
        }

        public Task<ServiceResult> SuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
            => SendStatusOnlyAsync(client => client.PutAsync($"api/admin/accounts/{accountId}/suspend", content: null, cancellationToken), cancellationToken);

        public Task<ServiceResult> UnsuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
            => SendStatusOnlyAsync(client => client.PutAsync($"api/admin/accounts/{accountId}/unsuspend", content: null, cancellationToken), cancellationToken);

        public Task<ServiceResult> UnlockAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
            => SendStatusOnlyAsync(client => client.PutAsync($"api/admin/accounts/{accountId}/unlock", content: null, cancellationToken), cancellationToken);

        public Task<ServiceResult> ResetPasswordAsync(Guid accountId, string newPassword, CancellationToken cancellationToken = default)
        {
            var body = new ResetPasswordDataTransferObject { NewPassword = newPassword };
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync($"api/admin/accounts/{accountId}/reset-password", body, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        private Task<ServiceResult> SendStatusOnlyAsync(Func<HttpClient, Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                _ => send(client),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }
    }
}
