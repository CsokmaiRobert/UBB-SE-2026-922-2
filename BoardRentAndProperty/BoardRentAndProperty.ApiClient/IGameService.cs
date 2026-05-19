using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public interface IGameService
    {
        Task<ServiceResult> CreateGameAsync(GameDTO game, CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateGameAsync(int gameId, GameDTO game, CancellationToken cancellationToken = default);

        Task<ServiceResult<GameDTO>> DeleteGameAsync(int gameId, CancellationToken cancellationToken = default);

        Task<ServiceResult<GameDTO>> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameDTO>>> GetGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameDTO>>> GetAllGamesAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameDTO>>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<GameDTO>>> GetActiveGamesForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);
    }
}
