using System;
using System.Net;
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
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("api/auth/login", body, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ProxyServiceException("Cannot connect to the API.", HttpStatusCode.ServiceUnavailable, apiErrorCode: null, ex);
            }

            using (response)
            {
                return await HttpResponseEnsurer.ReadJsonAsync<AccountProfileDataTransferObject>(response, cancellationToken);
            }
        }

        public async Task RegisterAsync(RegisterDataTransferObject body, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("api/auth/register", body, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ProxyServiceException("Cannot connect to the API.", HttpStatusCode.ServiceUnavailable, apiErrorCode: null, ex);
            }

            using (response)
            {
                await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
            }
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("api/auth/logout", content: null, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ProxyServiceException("Cannot connect to the API.", HttpStatusCode.ServiceUnavailable, apiErrorCode: null, ex);
            }

            using (response)
            {
                await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
            }
        }

        public async Task<string> ForgotPasswordAsync(CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync("api/auth/forgot-password", cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ProxyServiceException("Cannot connect to the API.", HttpStatusCode.ServiceUnavailable, apiErrorCode: null, ex);
            }

            using (response)
            {
                await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
        }
    }
}
