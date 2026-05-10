using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.Services
{
    public interface IGameProxyService
    {
        Task<IReadOnlyList<GameDTO>> GetAllGamesAsync();

        Task<GameDTO?> GetGameByIdAsync(int gameId);

        Task CreateGameAsync(GameDTO body);

        Task UpdateGameAsync(int gameId, GameDTO body);

        Task DeleteGameAsync(int gameId);
    }
}
