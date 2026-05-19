using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class GameService : ApiServiceBase, IGameService
    {
        public GameService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult> CreateGameAsync(GameDTO game, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsJsonAsync("api/games", game, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult> UpdateGameAsync(int gameId, GameDTO game, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PutAsJsonAsync($"api/games/{gameId}", game, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<GameDTO>> DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<GameDTO>(
                token => client.DeleteAsync($"api/games/{gameId}", token),
                async (response, token) =>
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return await ApiResponseReader.ToFailAsync<GameDTO>(response, token);
                    }

                    var parsed = await ApiResponseReader.ReadJsonAsync<GameDTO>(response, token);
                    return parsed.Success ? parsed : ServiceResult<GameDTO>.Ok(new GameDTO { Id = gameId });
                },
                cancellationToken);
        }

        public Task<ServiceResult<GameDTO>> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<GameDTO>(
                token => client.GetAsync($"api/games/{gameId}", token),
                (response, token) => ApiResponseReader.ReadJsonAsync<GameDTO>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/games/owner/{ownerAccountId}", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetAllGamesAsync(CancellationToken cancellationToken = default)
            => FetchListAsync("api/games", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/games/renter/{renterAccountId}/available", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetActiveGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/games/owner/{ownerAccountId}/active", cancellationToken);

        private Task<ServiceResult<IReadOnlyList<GameDTO>>> FetchListAsync(string requestPath, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<GameDTO>>(
                token => client.GetAsync(requestPath, token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<GameDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<GameDTO>>.Ok(parsed.Data ?? new List<GameDTO>())
                        : ServiceResult<IReadOnlyList<GameDTO>>.Fail(parsed);
                },
                cancellationToken);
        }
    }
}
