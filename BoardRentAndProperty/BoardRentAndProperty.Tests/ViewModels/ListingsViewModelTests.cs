using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class ListingsViewModelTests
    {
        private readonly Guid ownerUserId = Guid.NewGuid();
        private FakeClientGameService gameService = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService();
        }

        [Test]
        public void Constructor_LoadsGamesForOwner()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1), BuildGame(2), BuildGame(3));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(3));
        }

        [Test]
        public void Constructor_NoGames_TotalCountIsZero()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public void ShowingText_ContainsGameCountAndGamesWord()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1), BuildGame(2));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("2"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        [Test]
        public void LoadGames_RefreshesCollectionFromService()
        {
            var viewModel = BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(0));

            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(10), BuildGame(11));

            viewModel.LoadGames();

            Assert.That(viewModel.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public void DeleteGame_CallsServiceDeleteWithCorrectId()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(42));

            var viewModel = BuildViewModel();
            GameDTO gameToDelete = viewModel.PagedItems.First();

            viewModel.DeleteGame(gameToDelete);

            Assert.That(this.gameService.DeleteGameCallCount, Is.EqualTo(1));
            Assert.That(this.gameService.LastDeletedGameId, Is.EqualTo(42));
        }

        [Test]
        public void DeleteGame_ReloadsListAfterDeletion()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1), BuildGame(2));

            var viewModel = BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(2));

            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(2));

            viewModel.DeleteGame(BuildGame(1));

            Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void TryDeleteGame_SuccessfulDeletion_ReturnsSuccessWithGameRemovedTitle()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));

            var viewModel = BuildViewModel();
            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DialogTitle, Is.EqualTo("Game Removed"));
        }

        [Test]
        public void TryDeleteGame_GameHasActiveRentals_ReturnsFailureWithCannotDeleteTitle()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            this.gameService.DeleteGameException =
                new InvalidOperationException("There are 2 active rentals for this game and it cannot be removed now.");

            var viewModel = BuildViewModel();
            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Does.Contain("active rentals"));
        }

        [Test]
        public void TryDeleteGame_UnexpectedExceptionWithMessage_ReturnsFailureWithThatMessage()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            this.gameService.DeleteGameException = new Exception("Database connection failed.");

            var viewModel = BuildViewModel();
            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Is.EqualTo("Database connection failed."));
        }

        [Test]
        public void TryDeleteGame_UnexpectedExceptionWithEmptyMessage_ReturnsFallbackMessage()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            this.gameService.DeleteGameException = new Exception(string.Empty);

            var viewModel = BuildViewModel();
            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public void TryDeleteGame_UnexpectedExceptionWithWhitespaceMessage_ReturnsFallbackMessage()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            this.gameService.DeleteGameException = new Exception("   ");

            var viewModel = BuildViewModel();
            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public void PagedItems_MoreGamesThanPageSize_ShowsOnlyFirstPage()
        {
            int pageSize = PagedViewModel<GameDTO>.PageSize;
            var games = Enumerable.Range(1, pageSize + 2).Select(BuildGame).ToImmutableList();
            this.gameService.GamesForOwner = games;

            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(pageSize + 2));
            Assert.That(viewModel.PagedItems.Count, Is.LessThanOrEqualTo(pageSize));
        }

        [Test]
        public void ShowingText_WithGames_IncludesDisplayedAndTotalCounts()
        {
            var games = Enumerable.Range(1, 5).Select(BuildGame).ToImmutableList();
            this.gameService.GamesForOwner = games;

            var viewModel = BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("5"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        private ListingsViewModel BuildViewModel()
        {
            return new ListingsViewModel(this.gameService, this.ownerUserId);
        }

        private GameDTO BuildGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = this.ownerUserId },
                Name = $"Game {gameId}",
                Price = 9.99m,
                IsActive = true,
                Description = "Test game description.",
            };
        }
    }
}
