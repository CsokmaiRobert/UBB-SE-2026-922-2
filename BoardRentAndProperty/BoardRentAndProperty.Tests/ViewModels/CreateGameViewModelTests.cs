using System;
using System.Collections.Generic;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateGameViewModelTests
    {
        private readonly Guid testUserId = Guid.NewGuid();
        private FakeClientGameService gameService = null!;
        private FakeCurrentUserContext currentUserContext = null!;
        private CreateGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService
            {
                ValidateGameHandler = game => BoardRentAndProperty.Api.Services.GameInputHelper.BuildValidationErrors(
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
                    DomainConstants.GameMaximumDescriptionLength),
            };
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.testUserId };

            this.viewModel = new CreateGameViewModel(this.gameService, this.currentUserContext);
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
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(1));
            Assert.That(this.gameService.LastAddedGame!.Owner.Id, Is.EqualTo(this.testUserId));
            Assert.That(this.gameService.LastAddedGame.Name, Is.EqualTo("Settlers of Catan"));
            Assert.That(this.gameService.LastAddedGame.Price, Is.EqualTo(15.99m));

            this.gameService = new FakeClientGameService
            {
                ValidateGameHandler = this.gameService.ValidateGameHandler,
            };
            this.viewModel = new CreateGameViewModel(this.gameService, this.currentUserContext);
            PopulateWithValidInputs();
            this.viewModel.GameName = string.Empty;

            ViewOperationResult failureResult = this.viewModel.SubmitCreateGame();

            Assert.Multiple(() =>
            {
                Assert.That(failureResult.IsSuccess, Is.False);
                Assert.That(failureResult.DialogTitle, Is.EqualTo("Validation Error"));
            });
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(0));
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
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(1));

            this.gameService = new FakeClientGameService
            {
                ValidateGameHandler = this.gameService.ValidateGameHandler,
            };
            this.viewModel = new CreateGameViewModel(this.gameService, this.currentUserContext);
            PopulateWithValidInputs();
            this.viewModel.GameName = string.Empty;

            GameDTO invalidGame = this.viewModel.SaveGame();

            Assert.That(invalidGame, Is.Null);
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(0));
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
