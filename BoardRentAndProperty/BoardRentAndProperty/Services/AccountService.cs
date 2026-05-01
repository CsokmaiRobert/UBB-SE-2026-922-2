using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Services
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient httpClient;
        private readonly ISessionContext sessionContext;

        public AccountService(HttpClient httpClient, ISessionContext sessionContext)
        {
            this.httpClient = httpClient;
            this.sessionContext = sessionContext;
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId)
        {
            var response = await this.httpClient.GetAsync($"api/accounts/{accountId}");
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail(await ReadErrorAsync(response));
            }

            var profile = await response.Content.ReadFromJsonAsync<AccountProfileDataTransferObject>();
            if (profile == null)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail("Profile response was empty.");
            }

            ApiUrlHelper.RebaseAvatarUrl(this.httpClient.BaseAddress!, profile);
            return ServiceResult<AccountProfileDataTransferObject>.Ok(profile);
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData)
        {
            var response = await this.httpClient.PutAsJsonAsync($"api/accounts/{accountId}", profileUpdateData);
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.Fail(await ReadErrorAsync(response));
            }

            if (this.sessionContext.AccountId == accountId)
            {
                var refreshed = await this.GetProfileAsync(accountId);
                if (refreshed.Success && refreshed.Data != null)
                {
                    this.sessionContext.Populate(refreshed.Data);
                }
            }

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            var body = new ChangePasswordDataTransferObject
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
            };

            var response = await this.httpClient.PutAsJsonAsync($"api/accounts/{accountId}/password", body);
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.Fail(await ReadErrorAsync(response));
            }

            this.sessionContext.Clear();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<string> UploadAvatarAsync(Guid accountId, string sourceFilePath)
        {
            using var multipartContent = new MultipartFormDataContent();
            byte[] fileBytes = await File.ReadAllBytesAsync(sourceFilePath);
            var byteContent = new ByteArrayContent(fileBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(sourceFilePath));
            multipartContent.Add(byteContent, "file", Path.GetFileName(sourceFilePath));

            var response = await this.httpClient.PostAsync($"api/accounts/{accountId}/avatar", multipartContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadErrorAsync(response));
            }

            var payload = await response.Content.ReadFromJsonAsync<AvatarUploadResponseDataTransferObject>();
            string relativeUrl = payload?.AvatarUrl ?? string.Empty;
            return ApiUrlHelper.ToAbsoluteUrl(this.httpClient.BaseAddress!, relativeUrl);
        }

        public async Task RemoveAvatarAsync(Guid accountId)
        {
            var response = await this.httpClient.DeleteAsync($"api/accounts/{accountId}/avatar");
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadErrorAsync(response));
            }
        }

        private static string GuessContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream",
            };
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
