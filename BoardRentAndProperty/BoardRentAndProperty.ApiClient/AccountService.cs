using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class AccountService : ApiServiceBase, IAccountService
    {
        public AccountService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<AccountProfileDataTransferObject>(
                token => client.GetAsync($"api/accounts/{accountId}", token),
                async (response, token) =>
                {
                    var result = await ApiResponseReader.ReadJsonAsync<AccountProfileDataTransferObject>(response, token);
                    if (result.Success && result.Data is not null)
                    {
                        ApiUrlHelper.RebaseAvatarUrl(client.BaseAddress!, result.Data);
                    }

                    return result;
                },
                cancellationToken);
        }

        public Task<ServiceResult> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync($"api/accounts/{accountId}", profileUpdateData, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var body = new ChangePasswordDataTransferObject
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
            };

            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync($"api/accounts/{accountId}/password", body, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public async Task<ServiceResult<string>> UploadAvatarAsync(Guid accountId, string sourceFilePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = CreateClient();
                using var multipartContent = new MultipartFormDataContent();
                byte[] fileBytes = await File.ReadAllBytesAsync(sourceFilePath, cancellationToken);
                var byteContent = new ByteArrayContent(fileBytes);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(sourceFilePath));
                multipartContent.Add(byteContent, "file", Path.GetFileName(sourceFilePath));

                using var response = await client.PostAsync($"api/accounts/{accountId}/avatar", multipartContent, cancellationToken);
                var result = await ApiResponseReader.ReadJsonAsync<AvatarUploadResponseDataTransferObject>(response, cancellationToken);
                if (!result.Success)
                {
                    return ServiceResult<string>.Fail(result);
                }

                string relativeUrl = result.Data?.AvatarUrl ?? string.Empty;
                return ServiceResult<string>.Ok(ApiUrlHelper.ToAbsoluteUrl(client.BaseAddress!, relativeUrl));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                return ApiResponseReader.FromException<string>(exception);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return ServiceResult<string>.Fail($"Could not read the selected file. {exception.Message}");
            }
        }

        public Task<ServiceResult> RemoveAvatarAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.DeleteAsync($"api/accounts/{accountId}/avatar", token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
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
    }
}
