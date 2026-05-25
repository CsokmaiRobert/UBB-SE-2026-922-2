using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
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
            _ = this.ReloadAsync();
        }

        public ListingsViewModel(IGameService gameListingService, Guid currentAccountId)
            : this(gameListingService, new FixedDesktopAuthorizationService(currentAccountId))
        {
        }

        public string PageTitle => this.authorizationService.IsAdministrator ? "Games" : "My Listings";

        public Task LoadGamesAsync() => this.ReloadAsync();

        protected override void Reload()
        {
            _ = this.ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            if (!this.authorizationService.IsLoggedIn)
            {
                this.SetAllItems(ImmutableList<GameDTO>.Empty);
                return;
            }

            var gameListingsResult = this.authorizationService.IsAdministrator
                ? await this.gameListingService.GetAllGamesAsync()
                : await this.gameListingService.GetGamesForOwnerAsync(this.authorizationService.CurrentAccountId);

            this.SetAllItems(gameListingsResult.Success && gameListingsResult.Data != null
                ? gameListingsResult.Data.ToImmutableList()
                : ImmutableList<GameDTO>.Empty);
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} games";

        public async Task DeleteGameAsync(GameDTO gameToDelete)
        {
            if (!this.CanManageGame(gameToDelete))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

            var deleteResult = await this.gameListingService.DeleteGameAsync(gameToDelete.Id);
            if (!deleteResult.Success)
            {
                throw new InvalidOperationException(deleteResult.Error ?? Constants.DialogMessages.UnexpectedErrorOccurred);
            }

            await this.ReloadAsync();
        }

        [SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "The view model converts delete failures into a user-facing dialog result.")]
        public async Task<ViewOperationResult> TryDeleteGameAsync(GameDTO gameToDelete)
        {
            try
            {
                await this.DeleteGameAsync(gameToDelete);
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

        private sealed class FixedDesktopAuthorizationService : IDesktopAuthorizationService
        {
            private readonly Guid currentAccountId;

            public FixedDesktopAuthorizationService(Guid currentAccountId)
            {
                this.currentAccountId = currentAccountId;
            }

            public Guid CurrentAccountId => this.currentAccountId;

            public bool IsLoggedIn => true;

            public bool IsAdministrator => false;

            public bool CanAccessPage(Type pageType) => true;

            public bool CanAccessMenuPage(AppPage page) => true;
        }
    }
}
