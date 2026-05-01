using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface IGameService
    {
        void AddGame(GameDTO gameDto);
        void UpdateGameByIdentifier(int gameId, GameDTO updatedGameDTO);
        GameDTO DeleteGameByIdentifier(int gameId);
        GameDTO GetGameByIdentifier(int gameId);
        ImmutableList<GameDTO> GetGamesForOwner(Guid ownerAccountId);
        ImmutableList<GameDTO> GetAllGames();
        List<string> ValidateGame(GameDTO gameDto);
        ImmutableList<GameDTO> GetAvailableGamesForRenter(Guid renterAccountId);
        ImmutableList<GameDTO> GetActiveGamesForOwner(Guid ownerAccountId);
    }
}
