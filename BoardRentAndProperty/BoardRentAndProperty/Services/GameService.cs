using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
namespace BoardRentAndProperty.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository gameListingRepository;
        private readonly IRentalRepository gameRentalRepository;
        private readonly IMapper<Game, GameDTO, int> gameDtoMapper;
        private readonly IRequestService rentalRequestService;
        private const int NoActiveOrUpcomingRentals = 0;
        private const int SingularRentalCount = 1;
        public GameService(IGameRepository gameRepository, IRentalRepository rentalRepository, IMapper<Game, GameDTO, int> gameMapper, IRequestService requestService)
        {
            this.gameListingRepository = gameRepository;
            this.gameRentalRepository = rentalRepository;
            this.gameDtoMapper = gameMapper;
            this.rentalRequestService = requestService;
        }
        public List<string> ValidateGame(GameDTO gameDto) =>
            GameInputHelper.BuildValidationErrors(gameDto.Name, gameDto.Price, gameDto.MinimumPlayerNumber, gameDto.MaximumPlayerNumber, gameDto.Description,
                DomainConstants.GameMinimumNameLength, DomainConstants.GameMaximumNameLength, DomainConstants.GameMinimumAllowedPrice,
                DomainConstants.GameMinimumPlayerCount, DomainConstants.GameMinimumDescriptionLength, DomainConstants.GameMaximumDescriptionLength);
        public void AddGame(GameDTO gameToAdd)
        {
            var errors = ValidateGame(gameToAdd);
            if (errors.Count > NoActiveOrUpcomingRentals)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }
            gameToAdd.Image = GameInputHelper.EnsureImageOrDefault(gameToAdd.Image, AppDomain.CurrentDomain.BaseDirectory);
            gameListingRepository.Add(gameDtoMapper.ToModel(gameToAdd));
        }
        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameData)
        {
            var errors = ValidateGame(updatedGameData);
            if (errors.Count > NoActiveOrUpcomingRentals)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }
            updatedGameData.Image = GameInputHelper.EnsureImageOrDefault(updatedGameData.Image, AppDomain.CurrentDomain.BaseDirectory);
            gameListingRepository.Update(gameId, gameDtoMapper.ToModel(updatedGameData));
        }
        public GameDTO DeleteGameByIdentifier(int gameId)
        {
            var gameRentals = gameRentalRepository.GetRentalsByGame(gameId);
            var now = DateTime.Now;
            var activeCount = gameRentals.Count(r => r.EndDate >= now);
            if (activeCount > NoActiveOrUpcomingRentals)
            {
                var word = activeCount == SingularRentalCount ? "rental" : "rentals";
                throw new InvalidOperationException($"There are {activeCount} active {word} for this game and it cannot be removed now.");
            }
            foreach (var r in gameRentals)
            {
                gameRentalRepository.Delete(r.Id);
            }
            rentalRequestService.OnGameDeactivated(gameId);
            return gameDtoMapper.ToDTO(gameListingRepository.Delete(gameId));
        }
        public GameDTO GetGameByIdentifier(int gameId) => gameDtoMapper.ToDTO(gameListingRepository.Get(gameId));
        public ImmutableList<GameDTO> GetGamesForOwner(Guid ownerAccountId) =>
            gameListingRepository.GetGamesByOwner(ownerAccountId).Select(g => gameDtoMapper.ToDTO(g)).ToImmutableList();
        public ImmutableList<GameDTO> GetAllGames() =>
            gameListingRepository.GetAll().Select(g => gameDtoMapper.ToDTO(g)).ToImmutableList();
        public ImmutableList<GameDTO> GetAvailableGamesForRenter(Guid renterAccountId) =>
            GetAllGames().Where(g => g.IsActive && g.Owner?.Id != renterAccountId).ToImmutableList();
        public ImmutableList<GameDTO> GetActiveGamesForOwner(Guid ownerAccountId) =>
            GetGamesForOwner(ownerAccountId).Where(g => g.IsActive).ToImmutableList();
    }
}
