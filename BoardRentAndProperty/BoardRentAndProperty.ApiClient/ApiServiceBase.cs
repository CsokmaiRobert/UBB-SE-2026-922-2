using System.Net.Http;

namespace BoardRentAndProperty.ApiClient
{
    public abstract class ApiServiceBase
    {
        private readonly IHttpClientFactory httpClientFactory;

        protected ApiServiceBase(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected HttpClient CreateClient() => this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
    }
}
