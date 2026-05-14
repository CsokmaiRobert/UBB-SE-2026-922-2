using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;

namespace GUI_BRAP.ProxyServices
{
    public sealed class RentalProxyService : IRentalProxyService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public RentalProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync($"api/rentals/owner/{ownerAccountId}", cancellationToken);
            var rentals = await HttpResponseEnsurer.ReadJsonAsync<List<RentalDTO>>(response, cancellationToken);
            return rentals;
        }

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync($"api/rentals/renter/{renterAccountId}", cancellationToken);
            var rentals = await HttpResponseEnsurer.ReadJsonAsync<List<RentalDTO>>(response, cancellationToken);
            return rentals;
        }
    }
}
