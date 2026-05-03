using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateRequestViewModelTests
    {
        private readonly Guid currentUserId = Guid.NewGuid();
        private readonly Guid otherOwnerId = Guid.NewGuid();

        private Mock<IGameService> gameServiceMock = null!;
        private Mock<IRequestService> requestServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameServiceMock = new Mock<IGameService>();
            this.requestServiceMock = new Mock<IRequestService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();

            this.currentUserContextMock.SetupGet(context => context.CurrentUserId).Returns(this.currentUserId);
            this.gameServiceMock
                .Setup(service => service.GetAvailableGamesForRenter(this.currentUserId))
                .Returns(ImmutableList.Create(BuildOtherUsersGame(300)));
        }

        [Test]
        public void Constructor_LoadsGamesCurrentUserAndRefreshesCollection()
        {
            var viewModel = BuildViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.CurrentUserId, Is.EqualTo(this.currentUserId));
                Assert.That(viewModel.AvailableGamesToRequest.Count, Is.EqualTo(1));
                Assert.That(viewModel.AvailableGamesToRequest[0].Id, Is.EqualTo(300));
                Assert.That(viewModel.AvailableGamesToRequest[0].Owner.Id, Is.Not.EqualTo(this.currentUserId));
                Assert.That(viewModel.AvailableGamesToRequest[0].IsActive, Is.True);
            });

            this.gameServiceMock
                .Setup(service => service.GetAvailableGamesForRenter(this.currentUserId))
                .Returns(ImmutableList.Create(BuildOtherUsersGame(300), BuildOtherUsersGame(401)));

            viewModel.LoadAvailableGames();

            Assert.That(viewModel.AvailableGamesToRequest.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateRequestInputs_RequiresGameAndDates()
        {
            var viewModel = BuildViewModel();

            PopulateWithValidSelections(viewModel);
            Assert.That(viewModel.ValidateRequestInputs(), Is.True);

            AssertInvalidRequestInputs(viewModel, model => model.SelectedGame = null);
            AssertInvalidRequestInputs(viewModel, model => model.StartDate = null);
            AssertInvalidRequestInputs(viewModel, model => model.EndDate = null);
        }

        [Test]
        public void SubmitRequest_CoversValidationSuccessAndInvalidDateRange()
        {
            var invalidViewModel = BuildViewModel();

            ViewOperationResult validationFailure = invalidViewModel.SubmitRequest();

            Assert.Multiple(() =>
            {
                Assert.That(validationFailure.IsSuccess, Is.False);
                Assert.That(validationFailure.DialogTitle, Is.EqualTo("Validation Error"));
            });
            this.requestServiceMock.Verify(service => service.CreateRequest(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.Never);

            this.requestServiceMock
                .Setup(service => service.CreateRequest(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Success(1));

            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);

            ViewOperationResult successResult = successfulViewModel.SubmitRequest();

            Assert.That(successResult.IsSuccess, Is.True);
            this.requestServiceMock.Verify(service => service.CreateRequest(
                300,
                this.currentUserId,
                this.otherOwnerId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.Once);

            this.requestServiceMock
                .Setup(service => service.CreateRequest(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.InvalidDateRange));

            var invalidDateRangeViewModel = BuildViewModel();
            PopulateWithValidSelections(invalidDateRangeViewModel);

            ViewOperationResult invalidDateRangeResult = invalidDateRangeViewModel.SubmitRequest();

            Assert.Multiple(() =>
            {
                Assert.That(invalidDateRangeResult.IsSuccess, Is.False);
                Assert.That(invalidDateRangeResult.DialogTitle, Is.EqualTo("Validation Error"));
            });
        }

        [Test]
        public void SubmitRequest_MapsServiceErrorsAndTrySubmitRequestMirrorsResult()
        {
            this.requestServiceMock
                .Setup(service => service.CreateRequest(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent));

            var ownerCannotRentViewModel = BuildViewModel();
            PopulateWithValidSelections(ownerCannotRentViewModel);

            ViewOperationResult ownerCannotRentResult = ownerCannotRentViewModel.SubmitRequest();

            Assert.Multiple(() =>
            {
                Assert.That(ownerCannotRentResult.IsSuccess, Is.False);
                Assert.That(ownerCannotRentResult.DialogTitle, Is.EqualTo("Request Failed"));
                Assert.That(ownerCannotRentResult.DialogMessage, Does.Contain("own game"));
                Assert.That(ownerCannotRentViewModel.TrySubmitRequest(), Does.Contain("own game"));
            });

            this.requestServiceMock
                .Setup(service => service.CreateRequest(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable));

            var datesUnavailableViewModel = BuildViewModel();
            PopulateWithValidSelections(datesUnavailableViewModel);
            Assert.That(datesUnavailableViewModel.SubmitRequest().DialogMessage, Does.Contain("not available"));

            this.requestServiceMock
                .Setup(service => service.CreateRequest(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist));

            var missingGameViewModel = BuildViewModel();
            PopulateWithValidSelections(missingGameViewModel);
            Assert.That(missingGameViewModel.SubmitRequest().DialogMessage, Does.Contain("no longer exists"));

            this.requestServiceMock
                .Setup(service => service.CreateRequest(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Success(1));

            var successfulTrySubmitViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulTrySubmitViewModel);
            Assert.That(successfulTrySubmitViewModel.TrySubmitRequest(), Is.Null);
        }

        [Test]
        public void Setters_RaisePropertyChangedForBindableFields()
        {
            var viewModel = BuildViewModel();
            var changedProperties = new List<string?>();
            viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

            viewModel.SelectedGame = BuildOtherUsersGame(888);
            viewModel.StartDate = DateTimeOffset.Now.AddDays(2);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(10);

            Assert.That(changedProperties, Is.EqualTo(new[]
            {
                nameof(viewModel.SelectedGame),
                nameof(viewModel.StartDate),
                nameof(viewModel.EndDate),
            }));
        }

        private CreateRequestViewModel BuildViewModel()
        {
            return new CreateRequestViewModel(
                this.gameServiceMock.Object,
                this.requestServiceMock.Object,
                this.currentUserContextMock.Object);
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
