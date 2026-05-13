using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;

namespace GUI_BRAP.ProxyServices
{
    public sealed class RequestProxyService : IRequestProxyService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public RequestProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IReadOnlyList<RequestDTO>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync($"api/requests/owner/{ownerAccountId}/open", cancellationToken);
            var requests = await HttpResponseEnsurer.ReadJsonAsync<List<RequestDTO>>(response, cancellationToken);
            return requests;
        }

        public async Task OfferGameAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.PutAsJsonAsync($"api/requests/{requestId}/offer", body, cancellationToken);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DenyRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.PutAsJsonAsync($"api/requests/{requestId}/deny", body, cancellationToken);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
        }
    }
}
