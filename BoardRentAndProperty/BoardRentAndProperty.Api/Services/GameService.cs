using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Constants;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Services
{
    public class GameService : IGameService
    {
        private const int NoActiveOrUpcomingRentals = 0;
        private const int SingularRentalCount = 1;

        private readonly IGameRepository gameListingRepository;
        private readonly IRentalRepository gameRentalRepository;
        private readonly GameMapper gameDtoMapper;
        private readonly IRequestService rentalRequestService;

        public GameService(IGameRepository gameRepository, IRentalRepository rentalRepository, GameMapper gameMapper, IRequestService requestService)
        {
            this.gameListingRepository = gameRepository;
            this.gameRentalRepository = rentalRepository;
            this.gameDtoMapper = gameMapper;
            this.rentalRequestService = requestService;
        }

        public List<string> ValidateGame(GameDTO gameDto) =>
            GameInputHelper.BuildValidationErrors(
                gameDto.Name,
                gameDto.Price,
                gameDto.MinimumPlayerNumber,
                gameDto.MaximumPlayerNumber,
                gameDto.Description,
                DomainConstants.GameMinimumNameLength,
                DomainConstants.GameMaximumNameLength,
                DomainConstants.GameMinimumAllowedPrice,
                DomainConstants.GameMinimumPlayerCount,
                DomainConstants.GameMinimumDescriptionLength,
                DomainConstants.GameMaximumDescriptionLength);

        public void AddGame(GameDTO gameToAdd)
        {
            var errors = ValidateGame(gameToAdd);
            if (errors.Count > NoActiveOrUpcomingRentals)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            gameToAdd.Image = GameInputHelper.EnsureImageOrDefault(gameToAdd.Image, AppDomain.CurrentDomain.BaseDirectory);
            this.gameListingRepository.Add(this.gameDtoMapper.ToModel(gameToAdd)!);
        }

        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameData)
        {
            var errors = ValidateGame(updatedGameData);
            if (errors.Count > NoActiveOrUpcomingRentals)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            updatedGameData.Image = GameInputHelper.EnsureImageOrDefault(updatedGameData.Image, AppDomain.CurrentDomain.BaseDirectory);
            this.gameListingRepository.Update(gameId, this.gameDtoMapper.ToModel(updatedGameData)!);
        }

        public GameDTO DeleteGameByIdentifier(int gameId)
        {
            var gameRentals = this.gameRentalRepository.GetRentalsByGame(gameId);
            var now = DateTime.Now;
            var activeCount = gameRentals.Count(r => r.EndDate >= now);
            if (activeCount > NoActiveOrUpcomingRentals)
            {
                var word = activeCount == SingularRentalCount ? "rental" : "rentals";
                throw new InvalidOperationException($"There are {activeCount} active {word} for this game and it cannot be removed now.");
            }

            foreach (var rental in gameRentals)
            {
                this.gameRentalRepository.Delete(rental.Id);
            }

            this.rentalRequestService.OnGameDeactivated(gameId);
            return this.gameDtoMapper.ToDTO(this.gameListingRepository.Delete(gameId))!;
        }

        public GameDTO GetGameByIdentifier(int gameId) =>
            this.gameDtoMapper.ToDTO(this.gameListingRepository.Get(gameId))!;

        public ImmutableList<GameDTO> GetGamesForOwner(Guid ownerAccountId) =>
            this.gameListingRepository.GetGamesByOwner(ownerAccountId).Select(game => this.gameDtoMapper.ToDTO(game)!).ToImmutableList();

        public ImmutableList<GameDTO> GetAllGames() =>
            this.gameListingRepository.GetAll().Select(game => this.gameDtoMapper.ToDTO(game)!).ToImmutableList();

        public ImmutableList<GameDTO> GetAvailableGamesForRenter(Guid renterAccountId) =>
            GetAllGames().Where(game => game.IsActive && game.Owner?.Id != renterAccountId).ToImmutableList();

        public ImmutableList<GameDTO> GetActiveGamesForOwner(Guid ownerAccountId) =>
            GetGamesForOwner(ownerAccountId).Where(game => game.IsActive).ToImmutableList();
    }
}
