using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Services
{
    public class AdminService : IAdminService
    {
        private const string AdminAccessDeniedMessage = "Unauthorized access. Administrator role is required.";

        private readonly HttpClient httpClient;
        private readonly IDesktopAuthorizationService authorizationService;

        public AdminService(HttpClient httpClient, IDesktopAuthorizationService authorizationService)
        {
            this.httpClient = httpClient;
            this.authorizationService = authorizationService;
        }

        public async Task<ServiceResult<List<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int pageNumber, int pageSize)
        {
            if (!this.authorizationService.IsAdministrator)
            {
                return ServiceResult<List<AccountProfileDataTransferObject>>.Fail(AdminAccessDeniedMessage);
            }

            var response = await this.httpClient.GetAsync($"api/admin/accounts?page={pageNumber}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<List<AccountProfileDataTransferObject>>.Fail(await ReadErrorAsync(response));
            }

            var profiles = await response.Content.ReadFromJsonAsync<List<AccountProfileDataTransferObject>>() ?? new List<AccountProfileDataTransferObject>();
            foreach (var profile in profiles)
            {
                ApiUrlHelper.RebaseAvatarUrl(this.httpClient.BaseAddress!, profile);
            }

            return ServiceResult<List<AccountProfileDataTransferObject>>.Ok(profiles);
        }

        public async Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId)
        {
            if (!this.authorizationService.IsAdministrator)
            {
                return ServiceResult<bool>.Fail(AdminAccessDeniedMessage);
            }

            var response = await this.httpClient.PutAsync($"api/admin/accounts/{accountId}/suspend", content: null);
            return await ToBoolResultAsync(response);
        }

        public async Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId)
        {
            if (!this.authorizationService.IsAdministrator)
            {
                return ServiceResult<bool>.Fail(AdminAccessDeniedMessage);
            }

            var response = await this.httpClient.PutAsync($"api/admin/accounts/{accountId}/unsuspend", content: null);
            return await ToBoolResultAsync(response);
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword)
        {
            if (!this.authorizationService.IsAdministrator)
            {
                return ServiceResult<bool>.Fail(AdminAccessDeniedMessage);
            }

            var body = new ResetPasswordDataTransferObject { NewPassword = newPassword };
            var response = await this.httpClient.PutAsJsonAsync($"api/admin/accounts/{accountId}/reset-password", body);
            return await ToBoolResultAsync(response);
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId)
        {
            if (!this.authorizationService.IsAdministrator)
            {
                return ServiceResult<bool>.Fail(AdminAccessDeniedMessage);
            }

            var response = await this.httpClient.PutAsync($"api/admin/accounts/{accountId}/unlock", content: null);
            return await ToBoolResultAsync(response);
        }

        private static async Task<ServiceResult<bool>> ToBoolResultAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.Fail(await ReadErrorAsync(response));
            }

            return ServiceResult<bool>.Ok(true);
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var errorEnvelope = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
                if (!string.IsNullOrEmpty(errorEnvelope?.Error))
                {
                    return errorEnvelope!.Error!;
                }
            }
            catch
            {
            }

            return $"Server returned status {(int)response.StatusCode}.";
        }

        private sealed class ErrorEnvelope
        {
            public string? Error { get; set; }
        }
    }
}
