using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
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

        public EditGameViewModel(IGameService gameListingService)
            : this(gameListingService, new AlwaysAuthorizedDesktopAuthorizationService())
        {
        }

        public async Task LoadGameAsync(int gameIdToLoad)
        {
            var loadedGameResult = await this.gameListingService.GetGameByIdAsync(gameIdToLoad);
            if (!loadedGameResult.Success || loadedGameResult.Data == null)
            {
                return;
            }

            var loadedGame = loadedGameResult.Data;
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
            return GameInputValidator.Validate(BuildUpdatedGameDataTransferObject());
        }

        public async Task<ViewOperationResult> SubmitGameUpdateAsync()
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

            var updateResult = await this.UpdateGameAsync();
            return updateResult != null
                ? ViewOperationResult.Success()
                : ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.UnexpectedErrorOccurred);
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

        public async Task<GameDTO?> UpdateGameAsync()
        {
            var updatedGameDataTransferObject = BuildUpdatedGameDataTransferObject();

            if (GameInputValidator.Validate(updatedGameDataTransferObject).Count > NoValidationErrors)
            {
                return null;
            }

            var updateGameResult = await this.gameListingService.UpdateGameAsync(
                EditedGameId,
                updatedGameDataTransferObject);

            return updateGameResult.Success ? updatedGameDataTransferObject : null;
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

        private sealed class AlwaysAuthorizedDesktopAuthorizationService : IDesktopAuthorizationService
        {
            public Guid CurrentAccountId => Guid.Empty;

            public bool IsLoggedIn => true;

            public bool IsAdministrator => true;

            public bool CanAccessPage(Type pageType) => true;

            public bool CanAccessMenuPage(AppPage page) => true;
        }
    }
}
