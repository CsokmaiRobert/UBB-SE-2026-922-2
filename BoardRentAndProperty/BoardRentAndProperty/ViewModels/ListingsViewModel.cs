using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;

namespace BoardRentAndProperty.ViewModels
{
    public class ListingsViewModel : PagedViewModel<GameDTO>
    {
        private const int NoActiveRentalsCount = 0;
        private const string DeleteSuccessMessageTemplate =
            "There are {0} active rentals for this game. It was removed successfully.";

        private readonly IGameService gameListingService;
        private readonly IDesktopAuthorizationService authorizationService;

        public ListingsViewModel(IGameService gameListingService, IDesktopAuthorizationService authorizationService)
        {
            this.gameListingService = gameListingService;
            this.authorizationService = authorizationService;
            Reload();
        }

        public string PageTitle => this.authorizationService.IsAdministrator ? "Games" : "My Listings";

        public void LoadGames() => Reload();

        protected override void Reload()
        {
            if (!this.authorizationService.IsLoggedIn)
            {
                SetAllItems(ImmutableList<GameDTO>.Empty);
                return;
            }

            var gameListings = this.authorizationService.IsAdministrator
                ? this.gameListingService.GetAllGames()
                : this.gameListingService.GetGamesForOwner(this.authorizationService.CurrentAccountId);

            SetAllItems(gameListings.ToImmutableList());
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} games";

        public void DeleteGame(GameDTO gameToDelete)
        {
            if (!this.CanManageGame(gameToDelete))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

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

        private bool CanManageGame(GameDTO gameToManage)
        {
            return this.authorizationService.IsAdministrator
                || gameToManage.Owner?.Id == this.authorizationService.CurrentAccountId;
        }
    }
}
