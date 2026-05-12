namespace GUI_BRAP.ProxyServices
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Contracts.DataTransferObjects;
    using GUI_BRAP.Infrastructure;

    public class AccountProxyService : IAccountProxyService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public AccountProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<AccountProfileDataTransferObject> GetProfileAsync(Guid accountId)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync($"api/accounts/{accountId}");

            return await HttpResponseEnsurer.ReadJsonAsync<AccountProfileDataTransferObject>(response, CancellationToken.None);
        }

        public async Task UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject updateData)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.PutAsJsonAsync($"api/accounts/{accountId}", updateData);

            await HttpResponseEnsurer.EnsureSuccessAsync(response, CancellationToken.None);
        }

        public async Task UploadAvatarAsync(Guid accountId, string imagePath)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using MultipartFormDataContent content = new MultipartFormDataContent();

            using FileStream fileStream = File.OpenRead(imagePath);
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(imagePath));

            using HttpResponseMessage response = await client.PostAsync($"api/accounts/{accountId}/avatar", content);

            await HttpResponseEnsurer.EnsureSuccessAsync(response, CancellationToken.None);
        }

        public async Task RemoveAvatarAsync(Guid accountId)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.DeleteAsync($"api/accounts/{accountId}/avatar");

            await HttpResponseEnsurer.EnsureSuccessAsync(response, CancellationToken.None);
        }

        public async Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);

            ChangePasswordDataTransferObject payload = new ChangePasswordDataTransferObject
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };

            using HttpResponseMessage response = await client.PutAsJsonAsync($"api/accounts/{accountId}/password", payload);

            await HttpResponseEnsurer.EnsureSuccessAsync(response, CancellationToken.None);
        }
    }
}