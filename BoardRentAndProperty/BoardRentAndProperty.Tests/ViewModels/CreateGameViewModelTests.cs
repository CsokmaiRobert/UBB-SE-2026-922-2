using System;
using System.Collections.Generic;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateGameViewModelTests
    {
        private readonly Guid testUserId = Guid.NewGuid();
        private Mock<IGameService> gameServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;
        private CreateGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameServiceMock = new Mock<IGameService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();
            this.currentUserContextMock
                .SetupGet(context => context.CurrentUserId)
                .Returns(this.testUserId);
            this.gameServiceMock
                .Setup(service => service.ValidateGame(It.IsAny<GameDTO>()))
                .Returns((GameDTO game) => BoardRentAndProperty.Api.Services.GameInputHelper.BuildValidationErrors(
                    game.Name,
                    game.Price,
                    game.MinimumPlayerNumber,
                    game.MaximumPlayerNumber,
                    game.Description,
                    DomainConstants.GameMinimumNameLength,
                    DomainConstants.GameMaximumNameLength,
                    DomainConstants.GameMinimumAllowedPrice,
                    DomainConstants.GameMinimumPlayerCount,
                    DomainConstants.GameMinimumDescriptionLength,
                    DomainConstants.GameMaximumDescriptionLength));

            this.viewModel = new CreateGameViewModel(this.gameServiceMock.Object, this.currentUserContextMock.Object);
        }

        [Test]
        public void Constructor_InitializesCurrentUserAndDefaultState()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentUserId, Is.EqualTo(this.testUserId));
                Assert.That(this.viewModel.GameName, Is.EqualTo(string.Empty));
                Assert.That(this.viewModel.GameDescription, Is.EqualTo(string.Empty));
                Assert.That(this.viewModel.IsGameActive, Is.True);
                Assert.That(this.viewModel.GameImage, Is.Null);
            });
        }

        [Test]
        public void ValidateGameInputs_CoversValidAndInvalidScenarios()
        {
            PopulateWithValidInputs();
            Assert.That(this.viewModel.ValidateGameInputs(), Is.Empty);

            AssertValidationError(model => model.GameName = "AB", "Name");
            AssertValidationError(model => model.GameName = string.Empty, "Name");
            AssertValidationError(model => model.GamePrice = 0m, "Price");
            AssertValidationError(model => model.MinimumPlayersRequired = 0, "player");
            AssertValidationError(model =>
            {
                model.MinimumPlayersRequired = 5;
                model.MaximumPlayersAllowed = 2;
            }, "Maximum");
            AssertValidationError(model => model.GameDescription = "Short", "Description");

            PopulateWithValidInputs();
            this.viewModel.GameName = string.Empty;
            this.viewModel.GamePrice = 0m;
            this.viewModel.GameDescription = string.Empty;

            List<string> errors = this.viewModel.ValidateGameInputs();

            Assert.That(errors.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PriceHelpers_ParseAndRoundTripValues()
        {
            this.viewModel.SetGamePriceFromText("25.50");
            Assert.That(this.viewModel.GamePrice, Is.EqualTo(25.50m));

            this.viewModel.GamePrice = 10m;
            this.viewModel.SetGamePriceFromText(string.Empty);
            Assert.That(this.viewModel.GamePrice, Is.EqualTo(0m));

            this.viewModel.GamePrice = 10m;
            this.viewModel.SetGamePriceFromText("not-a-price");
            Assert.That(this.viewModel.GamePrice, Is.EqualTo(0m));

            this.viewModel.GamePriceAsDouble = 19.99;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GamePrice, Is.EqualTo(19.99m));
                Assert.That(this.viewModel.GamePriceAsDouble, Is.EqualTo(19.99).Within(0.001));
            });
        }

        [Test]
        public void SubmitCreateGame_CoversSuccessAndValidationFailure()
        {
            PopulateWithValidInputs();

            ViewOperationResult successResult = this.viewModel.SubmitCreateGame();

            Assert.That(successResult.IsSuccess, Is.True);
            this.gameServiceMock.Verify(service => service.AddGame(It.Is<GameDTO>(game =>
                game.Owner.Id == this.testUserId &&
                game.Name == "Settlers of Catan" &&
                game.Price == 15.99m)), Times.Once);

            this.gameServiceMock.Invocations.Clear();
            this.viewModel.GameName = string.Empty;

            ViewOperationResult failureResult = this.viewModel.SubmitCreateGame();

            Assert.Multiple(() =>
            {
                Assert.That(failureResult.IsSuccess, Is.False);
                Assert.That(failureResult.DialogTitle, Is.EqualTo("Validation Error"));
            });
            this.gameServiceMock.Verify(service => service.AddGame(It.IsAny<GameDTO>()), Times.Never);
        }

        [Test]
        public void SaveGame_CoversSuccessAndValidationFailure()
        {
            PopulateWithValidInputs();

            GameDTO savedGame = this.viewModel.SaveGame();

            Assert.Multiple(() =>
            {
                Assert.That(savedGame, Is.Not.Null);
                Assert.That(savedGame.Owner.Id, Is.EqualTo(this.testUserId));
                Assert.That(savedGame.Name, Is.EqualTo("Settlers of Catan"));
                Assert.That(savedGame.Price, Is.EqualTo(15.99m));
                Assert.That(savedGame.MinimumPlayerNumber, Is.EqualTo(2));
                Assert.That(savedGame.MaximumPlayerNumber, Is.EqualTo(6));
            });
            this.gameServiceMock.Verify(service => service.AddGame(It.IsAny<GameDTO>()), Times.Once);

            this.gameServiceMock.Invocations.Clear();
            this.viewModel.GameName = string.Empty;

            GameDTO invalidGame = this.viewModel.SaveGame();

            Assert.That(invalidGame, Is.Null);
            this.gameServiceMock.Verify(service => service.AddGame(It.IsAny<GameDTO>()), Times.Never);
        }

        private void AssertValidationError(Action<CreateGameViewModel> mutate, string expectedMessageFragment)
        {
            PopulateWithValidInputs();
            mutate(this.viewModel);

            List<string> errors = this.viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain(expectedMessageFragment));
        }

        private void PopulateWithValidInputs()
        {
            this.viewModel.GameName = "Settlers of Catan";
            this.viewModel.GamePrice = 15.99m;
            this.viewModel.MinimumPlayersRequired = 2;
            this.viewModel.MaximumPlayersAllowed = 6;
            this.viewModel.GameDescription = "A classic resource-trading board game for families.";
        }
    }
}
