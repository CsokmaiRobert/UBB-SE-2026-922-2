using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface IGameProxyService
    {
        Task<IReadOnlyList<GameDTO>> GetAllGamesAsync(CancellationToken cancellationToken = default);

        Task<GameDTO?> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GameDTO>> GetGamesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

        Task CreateGameAsync(GameDTO body, CancellationToken cancellationToken = default);

        Task UpdateGameAsync(int gameId, GameDTO body, CancellationToken cancellationToken = default);

        Task DeleteGameAsync(int gameId, CancellationToken cancellationToken = default);
    }
}
