using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateRequestViewModelTests
    {
        private readonly Guid currentUserId = Guid.NewGuid();
        private readonly Guid otherOwnerId = Guid.NewGuid();

        private FakeClientGameService gameService = null!;
        private FakeClientRequestService requestService = null!;
        private FakeCurrentUserContext currentUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService
            {
                AvailableGamesForRenter = ImmutableList.Create(BuildOtherUsersGame(300)),
            };
            this.requestService = new FakeClientRequestService();
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.currentUserId };
        }

        [Test]
        public async Task Constructor_LoadsGamesAndRefreshesCollectionOnReload()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.AvailableGamesToRequest.Count, Is.EqualTo(1));
            Assert.That(viewModel.AvailableGamesToRequest[0].Owner.Id, Is.Not.EqualTo(this.currentUserId));

            this.gameService.AvailableGamesForRenter = ImmutableList.Create(BuildOtherUsersGame(300), BuildOtherUsersGame(401));
            await viewModel.LoadAvailableGamesAsync();
            Assert.That(viewModel.AvailableGamesToRequest.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateRequestInputs_RequiresGameAndDates()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            Assert.That(viewModel.ValidateRequestInputs(), Is.True);

            AssertInvalidRequestInputs(viewModel, model => model.SelectedGame = null!);
            AssertInvalidRequestInputs(viewModel, model => model.StartDate = null);
            AssertInvalidRequestInputs(viewModel, model => model.EndDate = null);
        }

        [Test]
        public async Task SubmitRequest_CoversValidationFailureAndSuccessfulCreation()
        {
            var invalidViewModel = BuildViewModel();
            ViewOperationResult validationFailure = await invalidViewModel.SubmitRequestAsync();
            Assert.That(validationFailure.DialogTitle, Is.EqualTo("Validation Error"));
            Assert.That(this.requestService.CreateRequestCallCount, Is.EqualTo(0));

            this.requestService.CreateRequestResult = Result<int, CreateRequestError>.Success(1);
            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);
            ViewOperationResult successResult = await successfulViewModel.SubmitRequestAsync();
            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(this.requestService.LastGameId, Is.EqualTo(300));
            Assert.That(this.requestService.LastOwnerAccountId, Is.EqualTo(this.otherOwnerId));
        }

        [Test]
        public async Task SubmitRequest_MapsAllServiceErrorPathsToFriendlyMessages()
        {
            this.requestService.CreateRequestResult = Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent);
            var ownerCannotRentViewModel = BuildViewModel();
            PopulateWithValidSelections(ownerCannotRentViewModel);
            var ownerCannotRentResult = await ownerCannotRentViewModel.SubmitRequestAsync();
            Assert.That(ownerCannotRentResult.DialogTitle, Is.EqualTo("Request Failed"));
            Assert.That(ownerCannotRentResult.DialogMessage, Does.Contain("own game"));

            this.requestService.CreateRequestResult = Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable);
            var datesUnavailableViewModel = BuildViewModel();
            PopulateWithValidSelections(datesUnavailableViewModel);
            var datesUnavailableResult = await datesUnavailableViewModel.SubmitRequestAsync();
            Assert.That(datesUnavailableResult.DialogMessage, Does.Contain("not available"));

            this.requestService.CreateRequestResult = Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist);
            var missingGameViewModel = BuildViewModel();
            PopulateWithValidSelections(missingGameViewModel);
            var missingGameResult = await missingGameViewModel.SubmitRequestAsync();
            Assert.That(missingGameResult.DialogMessage, Does.Contain("no longer exists"));

            this.requestService.CreateRequestResult = Result<int, CreateRequestError>.Success(1);
            var successfulTrySubmitViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulTrySubmitViewModel);
            string? successfulMessage = await successfulTrySubmitViewModel.TrySubmitRequestAsync();
            Assert.That(successfulMessage, Is.Null);
        }

        private CreateRequestViewModel BuildViewModel()
        {
            return new CreateRequestViewModel(this.gameService, this.requestService, this.currentUserContext);
        }

        private static void AssertInvalidRequestInputs(CreateRequestViewModel viewModel, Action<CreateRequestViewModel> invalidate)
        {
            PopulateWithValidSelections(viewModel);
            invalidate(viewModel);
            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

        private static void PopulateWithValidSelections(CreateRequestViewModel viewModel)
        {
            viewModel.SelectedGame = viewModel.AvailableGamesToRequest[0];
            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(7);
        }

        private GameDTO BuildOtherUsersGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = this.otherOwnerId },
                Name = $"Board Game {gameId}",
                Price = 12m,
                IsActive = true,
            };
        }
    }
}
