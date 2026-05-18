using System;
using System.Collections.Generic;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;

namespace BoardRentAndProperty.ViewModels
{
    public class EditGameViewModel
    {
        private static readonly Guid MissingOwnerId = Guid.Empty;
        private const int NoValidationErrors = 0;
        private const decimal ZeroPriceForEmptyOrInvalidInput = 0m;

        private readonly IGameService gameListingService;
        private readonly IDesktopAuthorizationService authorizationService;

        public int EditedGameId { get; private set; }
        public Guid EditedGameOwnerId { get; private set; }

        public string GameName { get; set; } = string.Empty;
        public decimal GamePrice { get; set; }
        public double GamePriceAsDouble
        {
            get => (double)GamePrice;
            set => GamePrice = (decimal)value;
        }
        public int MinimumPlayersRequired { get; set; } = DomainConstants.GameDefaultMinimumPlayers;
        public int MaximumPlayersAllowed { get; set; } = DomainConstants.GameDefaultMaximumPlayers;
        public string GameDescription { get; set; } = string.Empty;
        public bool IsGameActive { get; set; } = true;
        public byte[] GameImage { get; set; } = null;

        public bool HasGameImage => GameImage != null && GameImage.Length > 0;

        public EditGameViewModel(IGameService gameListingService, IDesktopAuthorizationService authorizationService)
        {
            this.gameListingService = gameListingService;
            this.authorizationService = authorizationService;
        }

        public void LoadGame(int gameIdToLoad)
        {
            var loadedGame = gameListingService.GetGameByIdentifier(gameIdToLoad);
            if (loadedGame == null)
            {
                return;
            }

            var loadedGameOwnerId = loadedGame.Owner?.Id ?? MissingOwnerId;
            if (!this.CanManageGame(loadedGameOwnerId))
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this game.");
            }

            EditedGameId = loadedGame.Id;
            EditedGameOwnerId = loadedGameOwnerId;

            GameName = loadedGame.Name;
            GamePrice = loadedGame.Price;
            MinimumPlayersRequired = loadedGame.MinimumPlayerNumber;
            MaximumPlayersAllowed = loadedGame.MaximumPlayerNumber;
            GameDescription = loadedGame.Description;
            IsGameActive = loadedGame.IsActive;
            GameImage = loadedGame.Image;
        }

        public List<string> ValidateGameInputs()
        {
            return gameListingService.ValidateGame(BuildUpdatedGameDataTransferObject());
        }

        public ViewOperationResult SubmitGameUpdate()
        {
            if (!this.CanManageGame(EditedGameOwnerId))
            {
                return ViewOperationResult.Failure(
                    "Access Denied",
                    "You are not authorized to edit this game.");
            }

            var gameValidationErrors = ValidateGameInputs();
            if (gameValidationErrors.Count > NoValidationErrors)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    string.Join(Environment.NewLine, gameValidationErrors));
            }

            UpdateGame();
            return ViewOperationResult.Success();
        }

        public void SetGamePriceFromText(string rawPriceText)
        {
            if (PriceInputParser.TryParsePriceInput(rawPriceText, out var parsedPriceAsDouble))
            {
                GamePriceAsDouble = parsedPriceAsDouble;
                return;
            }

            GamePrice = ZeroPriceForEmptyOrInvalidInput;
        }

        public GameDTO UpdateGame()
        {
            var updatedGameDataTransferObject = BuildUpdatedGameDataTransferObject();

            if (gameListingService.ValidateGame(updatedGameDataTransferObject).Count > NoValidationErrors)
            {
                return null;
            }

            gameListingService.UpdateGameByIdentifier(EditedGameId, updatedGameDataTransferObject);
            return updatedGameDataTransferObject;
        }

        private bool CanManageGame(Guid ownerAccountId)
        {
            return this.authorizationService.IsAdministrator
                || ownerAccountId == this.authorizationService.CurrentAccountId;
        }

        private GameDTO BuildUpdatedGameDataTransferObject()
        {
            return new GameDTO
            {
                Id = EditedGameId,
                Owner = new UserDTO { Id = EditedGameOwnerId },
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                Description = GameDescription,
                Image = GameImage,
                IsActive = IsGameActive
            };
        }
    }
}
