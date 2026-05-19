using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class CreateGameViewModel
    {
        private const int NoValidationErrors = 0;
        private const int NewGameId = 0;
        private const decimal ZeroPriceForEmptyOrInvalidInput = 0m;

        private readonly IGameService gameListingService;
        private readonly ICurrentUserContext currentUserContext;

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

        public Guid CurrentUserId => currentUserContext.CurrentUserId;

        public CreateGameViewModel(IGameService gameListingService, ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.currentUserContext = currentUserContext;
        }

        public List<string> ValidateGameInputs()
        {
            return GameInputValidator.Validate(BuildGameDataTransferObject());
        }

        public async Task<ViewOperationResult> SubmitCreateGameAsync()
        {
            var gameValidationErrors = ValidateGameInputs();
            if (gameValidationErrors.Count > NoValidationErrors)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    string.Join(Environment.NewLine, gameValidationErrors));
            }

            var savedGame = await this.SaveGameAsync();
            return savedGame != null
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

        public async Task<GameDTO?> SaveGameAsync()
        {
            var newGameDataTransferObject = BuildGameDataTransferObject();

            if (GameInputValidator.Validate(newGameDataTransferObject).Count > NoValidationErrors)
            {
                return null;
            }

            var createGameResult = await this.gameListingService.CreateGameAsync(newGameDataTransferObject);
            return createGameResult.Success ? newGameDataTransferObject : null;
        }

        private GameDTO BuildGameDataTransferObject()
        {
            return new GameDTO
            {
                Id = NewGameId,
                Owner = new UserDTO { Id = CurrentUserId },
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
