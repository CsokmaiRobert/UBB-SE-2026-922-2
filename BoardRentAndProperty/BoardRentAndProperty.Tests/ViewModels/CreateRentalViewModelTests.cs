using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateRentalViewModelTests
    {
        private readonly Guid ownerUserId = Guid.NewGuid();
        private readonly Guid renterUserId = Guid.NewGuid();

        private FakeClientGameService gameService = null!;
        private FakeClientRentalService rentalService = null!;
        private FakeClientUserService userService = null!;
        private FakeCurrentUserContext currentUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService
            {
                ActiveGamesForOwner = ImmutableList.Create(BuildActiveGame(100)),
            };
            this.rentalService = new FakeClientRentalService();
            this.userService = new FakeClientUserService
            {
                UsersExceptCurrent = ImmutableList.Create(new UserDTO { Id = this.renterUserId, DisplayName = "Renter" }),
            };
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.ownerUserId };
        }

        [Test]
        public async Task Constructor_LoadsCollectionsAndRefreshesData()
        {
            var viewModel = BuildViewModel();
            await viewModel.LoadRentalFormDataAsync();

            Assert.That(viewModel.OwnedActiveGames.Select(game => game.Id), Is.EquivalentTo(new[] { 100 }));
            Assert.That(viewModel.AvailableRenters.Select(user => user.Id), Is.EquivalentTo(new[] { this.renterUserId }));

            this.gameService.ActiveGamesForOwner = ImmutableList.Create(BuildActiveGame(100), BuildActiveGame(201));
            await viewModel.LoadRentalFormDataAsync();
            Assert.That(viewModel.OwnedActiveGames.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateRentalInputs_RequiresGameRenterAndDates()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            Assert.That(viewModel.ValidateRentalInputs(), Is.True);

            AssertInvalidRentalInputs(viewModel, model => model.SelectedGameToRent = null!);
            AssertInvalidRentalInputs(viewModel, model => model.SelectedRenter = null!);
            AssertInvalidRentalInputs(viewModel, model => model.StartDate = null);
            AssertInvalidRentalInputs(viewModel, model => model.EndDate = null);
        }

        [Test]
        public async Task CreateRental_CoversValidationFailureSuccessAndServiceFailureMessages()
        {
            var invalidViewModel = BuildViewModel();
            ViewOperationResult validationFailure = await invalidViewModel.CreateRentalAsync();
            Assert.That(validationFailure.DialogTitle, Is.EqualTo("Validation Error"));
            Assert.That(this.rentalService.CreateRentalCallCount, Is.EqualTo(0));

            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);
            ViewOperationResult successResult = await successfulViewModel.CreateRentalAsync();
            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(this.rentalService.LastGameId, Is.EqualTo(100));

            this.rentalService.CreateRentalException = new InvalidOperationException("Dates overlap with existing rental.");
            var failingViewModel = BuildViewModel();
            PopulateWithValidSelections(failingViewModel);
            ViewOperationResult failureResult = await failingViewModel.CreateRentalAsync();
            Assert.That(failureResult.DialogTitle, Is.EqualTo("Rental Failed"));
            Assert.That(failureResult.DialogMessage, Does.Contain("overlap"));
        }

        [Test]
        public async Task SaveRental_CoversSuccessValidationFailureAndServiceMessage()
        {
            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);
            Assert.That(await successfulViewModel.SaveRentalAsync(), Is.Null);

            var invalidViewModel = BuildViewModel();
            Assert.That(await invalidViewModel.SaveRentalAsync(), Is.EqualTo("Validation failed."));

            this.rentalService.CreateRentalException = new Exception("Database connection lost.");
            var failingViewModel = BuildViewModel();
            PopulateWithValidSelections(failingViewModel);
            Assert.That(await failingViewModel.SaveRentalAsync(), Is.EqualTo("Database connection lost."));
        }

        private CreateRentalViewModel BuildViewModel()
        {
            return new CreateRentalViewModel(this.gameService, this.rentalService, this.userService, this.currentUserContext);
        }

        private void AssertInvalidRentalInputs(CreateRentalViewModel viewModel, Action<CreateRentalViewModel> invalidate)
        {
            this.PopulateWithValidSelections(viewModel);
            invalidate(viewModel);
            Assert.That(viewModel.ValidateRentalInputs(), Is.False);
        }

        private void PopulateWithValidSelections(CreateRentalViewModel viewModel)
        {
            viewModel.SelectedGameToRent = BuildActiveGame(100);
            viewModel.SelectedRenter = new UserDTO { Id = this.renterUserId, DisplayName = "Renter" };
            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(7);
        }

        private GameDTO BuildActiveGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = this.ownerUserId },
                Name = "Test Game",
                Price = 10m,
                IsActive = true,
            };
        }
    }
}
