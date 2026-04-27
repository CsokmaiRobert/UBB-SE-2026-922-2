using System.Collections.Generic;
using System.Collections.Immutable;
using BoardRentAndProperty.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface IGameService
    {
        void AddGame(GameDTO gameDto);

        void UpdateGameByIdentifier(int gameId, GameDTO updatedGameDTO);

        GameDTO DeleteGameByIdentifier(int gameId);

        GameDTO GetGameByIdentifier(int gameId);

        ImmutableList<GameDTO> GetGamesForOwner(int ownerUserId);

        ImmutableList<GameDTO> GetAllGames();

        List<string> ValidateGame(GameDTO gameDto);

        ImmutableList<GameDTO> GetAvailableGamesForRenter(int renterUserId);

        ImmutableList<GameDTO> GetActiveGamesForOwner(int ownerUserId);
    }
}
