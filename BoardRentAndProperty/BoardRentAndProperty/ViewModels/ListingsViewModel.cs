using System.Collections.Immutable;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;

namespace BoardRentAndProperty.ViewModels
{
    public class ListingsViewModel : PagedViewModel<GameDTO>
    {
        private const int NoActiveRentalsCount = 0;
        private const string DeleteSuccessMessageTemplate =
            "There are {0} active rentals for this game. It was removed successfully.";

        private readonly IGameService gameListingService;
        private readonly int currentOwnerUserId;

        public ListingsViewModel(IGameService gameListingService, int currentOwnerUserId)
        {
            this.gameListingService = gameListingService;
            this.currentOwnerUserId = currentOwnerUserId;
            Reload();
        }

        public void LoadGames() => Reload();

        protected override void Reload()
        {
            var ownerGameListings = gameListingService.GetGamesForOwner(currentOwnerUserId);
            SetAllItems(ownerGameListings.ToImmutableList());
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} games";

        public void DeleteGame(GameDTO gameToDelete)
        {
            gameListingService.DeleteGameByIdentifier(gameToDelete.Id);
            Reload();
        }

        public ViewOperationResult TryDeleteGame(GameDTO gameToDelete)
        {
            try
            {
                DeleteGame(gameToDelete);
                return ViewOperationResult.Success(
                    Constants.DialogTitles.GameRemoved,
                    string.Format(DeleteSuccessMessageTemplate, NoActiveRentalsCount));
            }
            catch (System.InvalidOperationException gameHasActiveRentalsException)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.CannotDeleteGame,
                    gameHasActiveRentalsException.Message);
            }
            catch (System.Exception unexpectedException)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.CannotDeleteGame,
                    string.IsNullOrWhiteSpace(unexpectedException.Message)
                        ? Constants.DialogMessages.UnexpectedErrorOccurred
                        : unexpectedException.Message);
            }
        }
    }
}