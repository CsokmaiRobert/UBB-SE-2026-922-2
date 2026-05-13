using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync(CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync("api/games", cancellationToken);
            var games = await HttpResponseEnsurer.ReadJsonAsync<List<GameDTO>>(response, cancellationToken);
            return games;
        }

        public async Task<GameDTO?> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync($"api/games/{gameId}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await HttpResponseEnsurer.ReadJsonAsync<GameDTO>(response, cancellationToken);
        }

        public async Task<IReadOnlyList<GameDTO>> GetGamesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.GetAsync($"api/games/owner/{ownerId}", cancellationToken);
            var games = await HttpResponseEnsurer.ReadJsonAsync<List<GameDTO>>(response, cancellationToken);
            return games;
        }

        public async Task CreateGameAsync(GameDTO body, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.PostAsJsonAsync("api/games", body, cancellationToken);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdateGameAsync(int gameId, GameDTO body, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.PutAsJsonAsync($"api/games/{gameId}", body, cancellationToken);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
            using HttpResponseMessage response = await client.DeleteAsync($"api/games/{gameId}", cancellationToken);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, cancellationToken);
        }
    }
}
