using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;

namespace GUI_BRAP.ProxyServices
{
    public sealed class AuthProxyService : IAuthProxyService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public AuthProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<AccountProfileDataTransferObject> LoginAsync(LoginDataTransferObject body, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.PostAsJsonAsync("api/auth/login", body, cancellationToken);
            return await HttpResponseEnsurer.ReadJsonAsync<AccountProfileDataTransferObject>(response, cancellationToken);
        }
    }
}
