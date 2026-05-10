using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.Services
{
    public class GameProxyService : IGameProxyService
    {
        private const string ApiClientName = "BoardRentAndPropertyApi";

        private readonly IHttpClientFactory httpClientFactory;

        public GameProxyService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync()
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientName);
            HttpResponseMessage response = await client.GetAsync("api/games");
            await HttpResponseEnsurer.EnsureSuccessAsync(response, "Failed to load games.");
            List<GameDTO>? games = await response.Content.ReadFromJsonAsync<List<GameDTO>>();
            return games ?? new List<GameDTO>();
        }

        public async Task<GameDTO?> GetGameByIdAsync(int gameId)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientName);
            HttpResponseMessage response = await client.GetAsync($"api/games/{gameId}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            await HttpResponseEnsurer.EnsureSuccessAsync(response, "Failed to load game.");
            return await response.Content.ReadFromJsonAsync<GameDTO>();
        }

        public async Task CreateGameAsync(GameDTO body)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientName);
            HttpResponseMessage response = await client.PostAsJsonAsync("api/games", body);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, "Failed to create game.");
        }

        public async Task UpdateGameAsync(int gameId, GameDTO body)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientName);
            HttpResponseMessage response = await client.PutAsJsonAsync($"api/games/{gameId}", body);
            await HttpResponseEnsurer.EnsureSuccessAsync(response, "Failed to update game.");
        }

        public async Task DeleteGameAsync(int gameId)
        {
            HttpClient client = this.httpClientFactory.CreateClient(ApiClientName);
            HttpResponseMessage response = await client.DeleteAsync($"api/games/{gameId}");
            await HttpResponseEnsurer.EnsureSuccessAsync(response, "Failed to delete game.");
        }
    }
}
