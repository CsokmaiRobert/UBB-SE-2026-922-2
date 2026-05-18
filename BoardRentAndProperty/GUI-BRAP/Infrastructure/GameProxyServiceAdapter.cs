using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;

namespace GUI_BRAP.Infrastructure
{
    public sealed class GameProxyServiceAdapter : IGameProxyService
    {
        private readonly IGameService gameService;

        public GameProxyServiceAdapter(IGameService gameService)
        {
            this.gameService = gameService;
        }

        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync(CancellationToken cancellationToken = default)
            => (await this.gameService.GetAllGamesAsync(cancellationToken)).ThrowIfFailed();

        public async Task<GameDTO?> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            var result = await this.gameService.GetGameByIdAsync(gameId, cancellationToken);
            if (result.Success)
            {
                return result.Data;
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw ProxyResultExtensions.ToException(result);
        }

        public async Task<IReadOnlyList<GameDTO>> GetGamesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
            => (await this.gameService.GetGamesForOwnerAsync(ownerId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<GameDTO>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await this.gameService.GetAvailableGamesForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();

        public async Task CreateGameAsync(GameDTO body, CancellationToken cancellationToken = default)
            => (await this.gameService.CreateGameAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task UpdateGameAsync(int gameId, GameDTO body, CancellationToken cancellationToken = default)
            => (await this.gameService.UpdateGameAsync(gameId, body, cancellationToken)).ThrowIfFailed();

        public async Task DeleteGameAsync(int gameId, CancellationToken cancellationToken = default)
            => (await this.gameService.DeleteGameAsync(gameId, cancellationToken)).ThrowIfFailed();
    }
}
