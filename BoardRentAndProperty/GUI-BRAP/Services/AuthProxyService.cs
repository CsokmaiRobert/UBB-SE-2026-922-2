using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.Services
{
    public class AuthProxyService : IAuthProxyService
    {
        private const string ApiClientName = "BoardRentAndPropertyApi";

        private readonly IHttpClientFactory httpClientFactory;

        public AuthProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<AccountProfileDataTransferObject> LoginAsync(LoginDataTransferObject body)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientName);
            HttpResponseMessage response = await client.PostAsJsonAsync("api/auth/login", body);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, "Login failed.");

            AccountProfileDataTransferObject? profile = await response.Content.ReadFromJsonAsync<AccountProfileDataTransferObject>();
            if (profile is null)
            {
                throw new ProxyServiceException("Login response was empty.", (int)response.StatusCode);
            }

            return profile;
        }
    }
}
