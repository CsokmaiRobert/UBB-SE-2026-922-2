using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateRentalViewModelTests
    {
        private readonly Guid ownerUserId = Guid.NewGuid();
        private readonly Guid renterUserId = Guid.NewGuid();

        private Mock<IGameService> gameServiceMock = null!;
        private Mock<IRentalService> rentalServiceMock = null!;
        private Mock<IUserService> userServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameServiceMock = new Mock<IGameService>();
            this.rentalServiceMock = new Mock<IRentalService>();
            this.userServiceMock = new Mock<IUserService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();

            this.currentUserContextMock.SetupGet(context => context.CurrentUserId).Returns(this.ownerUserId);
            this.gameServiceMock
                .Setup(service => service.GetActiveGamesForOwner(this.ownerUserId))
                .Returns(ImmutableList.Create(BuildActiveGame(100)));
            this.userServiceMock
                .Setup(service => service.GetUsersExcept(this.ownerUserId))
                .Returns(ImmutableList.Create(new UserDTO { Id = this.renterUserId, DisplayName = "Renter" }));
        }

        [Test]
        public async Task Constructor_LoadsCollectionsCurrentUserAndRefreshesData()
        {
            var viewModel = BuildViewModel();
            await viewModel.LoadRentalFormDataAsync();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.CurrentUserId, Is.EqualTo(this.ownerUserId));
                Assert.That(viewModel.OwnedActiveGames.Select(game => game.Id), Is.EquivalentTo(new[] { 100 }));
                Assert.That(viewModel.OwnedActiveGames.All(game => game.IsActive), Is.True);
                Assert.That(viewModel.AvailableRenters.Select(user => user.Id), Is.EquivalentTo(new[] { this.renterUserId }));
            });

            this.gameServiceMock
                .Setup(service => service.GetActiveGamesForOwner(this.ownerUserId))
                .Returns(ImmutableList.Create(BuildActiveGame(100), BuildActiveGame(201)));
            this.userServiceMock
                .Setup(service => service.GetUsersExcept(this.ownerUserId))
                .Returns(ImmutableList.Create(
                    new UserDTO { Id = this.renterUserId, DisplayName = "Renter" },
                    new UserDTO { Id = Guid.NewGuid(), DisplayName = "Second renter" }));

            await viewModel.LoadRentalFormDataAsync();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.OwnedActiveGames.Select(game => game.Id), Is.EquivalentTo(new[] { 100, 201 }));
                Assert.That(viewModel.AvailableRenters.Count, Is.EqualTo(2));
            });
        }

        [Test]
        public void ValidateRentalInputs_RequiresGameRenterAndDates()
        {
            var viewModel = BuildViewModel();

            PopulateWithValidSelections(viewModel);
            Assert.That(viewModel.ValidateRentalInputs(), Is.True);

            AssertInvalidRentalInputs(viewModel, model => model.SelectedGameToRent = null);
            AssertInvalidRentalInputs(viewModel, model => model.SelectedRenter = null);
            AssertInvalidRentalInputs(viewModel, model => model.StartDate = null);
            AssertInvalidRentalInputs(viewModel, model => model.EndDate = null);
        }

        [Test]
        public void CreateRental_CoversSuccessValidationFailureAndExceptions()
        {
            var invalidViewModel = BuildViewModel();

            ViewOperationResult validationFailure = invalidViewModel.CreateRental();

            Assert.Multiple(() =>
            {
                Assert.That(validationFailure.IsSuccess, Is.False);
                Assert.That(validationFailure.DialogTitle, Is.EqualTo("Validation Error"));
            });
            this.rentalServiceMock.Verify(service => service.CreateConfirmedRental(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.Never);

            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);

            ViewOperationResult successResult = successfulViewModel.CreateRental();

            Assert.That(successResult.IsSuccess, Is.True);
            this.rentalServiceMock.Verify(service => service.CreateConfirmedRental(
                100,
                this.renterUserId,
                this.ownerUserId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.Once);

            this.rentalServiceMock
                .Setup(service => service.CreateConfirmedRental(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Throws(new ArgumentException("Start date must be before end date and not in the past."));

            var argumentExceptionViewModel = BuildViewModel();
            PopulateWithValidSelections(argumentExceptionViewModel);

            ViewOperationResult argumentExceptionResult = argumentExceptionViewModel.CreateRental();

            Assert.Multiple(() =>
            {
                Assert.That(argumentExceptionResult.IsSuccess, Is.False);
                Assert.That(argumentExceptionResult.DialogTitle, Is.EqualTo("Validation Error"));
            });

            this.rentalServiceMock
                .Setup(service => service.CreateConfirmedRental(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Throws(new InvalidOperationException("Dates overlap with existing rental."));

            var unexpectedExceptionViewModel = BuildViewModel();
            PopulateWithValidSelections(unexpectedExceptionViewModel);

            ViewOperationResult unexpectedExceptionResult = unexpectedExceptionViewModel.CreateRental();

            Assert.Multiple(() =>
            {
                Assert.That(unexpectedExceptionResult.IsSuccess, Is.False);
                Assert.That(unexpectedExceptionResult.DialogTitle, Is.EqualTo("Rental Failed"));
                Assert.That(unexpectedExceptionResult.DialogMessage, Does.Contain("overlap"));
            });
        }

        [Test]
        public void SaveRental_CoversSuccessValidationFailureAndServiceMessage()
        {
            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);

            string validationMessage = successfulViewModel.SaveRental();
            Assert.That(validationMessage, Is.Null);

            var invalidViewModel = BuildViewModel();
            string invalidResult = invalidViewModel.SaveRental();
            Assert.That(invalidResult, Is.EqualTo("Validation failed."));

            this.rentalServiceMock
                .Setup(service => service.CreateConfirmedRental(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Throws(new Exception("Database connection lost."));

            var failingViewModel = BuildViewModel();
            PopulateWithValidSelections(failingViewModel);

            string exceptionMessage = failingViewModel.SaveRental();
            Assert.That(exceptionMessage, Is.EqualTo("Database connection lost."));
        }

        [Test]
        public void Setters_RaisePropertyChangedForBindableFields()
        {
            var viewModel = BuildViewModel();
            var changedProperties = new List<string?>();
            viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

            viewModel.SelectedGameToRent = BuildActiveGame(999);
            viewModel.SelectedRenter = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Listener" };
            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(5);

            Assert.That(changedProperties, Is.EqualTo(new[]
            {
                nameof(viewModel.SelectedGameToRent),
                nameof(viewModel.SelectedRenter),
                nameof(viewModel.StartDate),
                nameof(viewModel.EndDate),
            }));
        }

        private CreateRentalViewModel BuildViewModel()
        {
            return new CreateRentalViewModel(
                this.gameServiceMock.Object,
                this.rentalServiceMock.Object,
                this.userServiceMock.Object,
                this.currentUserContextMock.Object);
        }

        private static void AssertInvalidRentalInputs(CreateRentalViewModel viewModel, Action<CreateRentalViewModel> invalidate)
        {
            PopulateWithValidSelections(viewModel);
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
