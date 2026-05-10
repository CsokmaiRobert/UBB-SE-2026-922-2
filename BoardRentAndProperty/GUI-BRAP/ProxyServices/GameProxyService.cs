using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;

namespace GUI_BRAP.ProxyServices
{
    public sealed class GameProxyService : IGameProxyService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public GameProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IReadOnlyList<GameDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync("api/games", cancellationToken);
            var games = await HttpResponseEnsurer.ReadJsonAsync<List<GameDTO>>(response, cancellationToken);
            return games;
        }
    }
}
