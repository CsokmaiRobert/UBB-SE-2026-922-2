using GUI_BRAP.Infrastructure;
using GUI_BRAP.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{

    public class AdminProxyService : IAdminProxyService
    {
        private readonly HttpClient _httpClient;

        public AdminProxyService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
        }

        public async Task<IEnumerable<AdminAccountViewModel>> GetAllAccountsAsync()
        {
            var response = await _httpClient.GetAsync("api/admin/accounts");
            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await response.Content.ReadFromJsonAsync<IEnumerable<AdminAccountViewModel>>(options)
                   ?? new List<AdminAccountViewModel>();
        }

        public async Task SuspendAccountAsync(string accountId)
        {
            var emptyContent = new StringContent(string.Empty);
            var response = await _httpClient.PutAsync($"api/admin/accounts/{accountId}/suspend", emptyContent);
            response.EnsureSuccessStatusCode();
        }

        public async Task UnsuspendAccountAsync(string accountId)
        {
            var emptyContent = new StringContent(string.Empty);
            var response = await _httpClient.PutAsync($"api/admin/accounts/{accountId}/unsuspend", emptyContent);
            response.EnsureSuccessStatusCode();
        }

        public async Task UnlockAccountAsync(string accountId)
        {
            var emptyContent = new StringContent(string.Empty);
            var response = await _httpClient.PutAsync($"api/admin/accounts/{accountId}/unlock", emptyContent);
            response.EnsureSuccessStatusCode();
        }


        public async Task ResetPasswordAsync(string accountId, string newPassword)
        {
            var payload = new ResetPasswordDataTransferObject
            {
                NewPassword = newPassword
            };

            var response = await _httpClient.PutAsJsonAsync($"api/admin/accounts/{accountId}/reset-password", payload);

            response.EnsureSuccessStatusCode();
        }
    }
}