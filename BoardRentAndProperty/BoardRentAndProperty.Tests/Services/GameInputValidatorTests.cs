using System;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Services
{
    [TestFixture]
    public sealed class GameInputValidatorTests
    {
        [Test]
        public void Validate_WithAllValidFields_ReturnsEmptyValidationErrorList()
        {
            var validGame = BuildBaseGame();

            var validationErrors = GameInputValidator.Validate(validGame);

            Assert.That(validationErrors, Is.Empty);
        }

        [Test]
        public void Validate_WithInvalidName_ReturnsNameError()
        {
            var gameWithBlankName = BuildBaseGame();
            gameWithBlankName.Name = "   ";

            var validationErrors = GameInputValidator.Validate(gameWithBlankName);

            Assert.That(validationErrors, Has.Some.Contains("Name"));
        }

        [Test]
        public void Validate_WithPriceBelowMinimum_ReturnsPriceError()
        {
            var gameWithLowPrice = BuildBaseGame();
            gameWithLowPrice.Price = DomainConstants.GameMinimumAllowedPrice - 0.5m;

            var validationErrors = GameInputValidator.Validate(gameWithLowPrice);

            Assert.That(validationErrors, Has.Some.Contains("Price"));
        }

        [Test]
        public void Validate_WithMaximumPlayerCountLowerThanMinimum_ReturnsOrderingError()
        {
            var gameWithBadPlayerOrdering = BuildBaseGame();
            gameWithBadPlayerOrdering.MinimumPlayerNumber = 5;
            gameWithBadPlayerOrdering.MaximumPlayerNumber = 3;

            var validationErrors = GameInputValidator.Validate(gameWithBadPlayerOrdering);

            Assert.That(validationErrors, Has.Some.Contains("Maximum player count"));
        }

        [Test]
        public void Validate_WithEmptyDescription_ReturnsDescriptionError()
        {
            var gameWithoutDescription = BuildBaseGame();
            gameWithoutDescription.Description = string.Empty;

            var validationErrors = GameInputValidator.Validate(gameWithoutDescription);

            Assert.That(validationErrors, Has.Some.Contains("Description"));
        }

        [Test]
        public void Validate_WithMultipleInvalidFields_ReturnsAllRelevantErrors()
        {
            var gameWithSeveralProblems = BuildBaseGame();
            gameWithSeveralProblems.Name = string.Empty;
            gameWithSeveralProblems.Price = -1m;
            gameWithSeveralProblems.MinimumPlayerNumber = 0;
            gameWithSeveralProblems.MaximumPlayerNumber = 0;
            gameWithSeveralProblems.Description = string.Empty;

            var validationErrors = GameInputValidator.Validate(gameWithSeveralProblems);

            Assert.That(validationErrors, Has.Count.GreaterThanOrEqualTo(4));
        }

        private static GameDTO BuildBaseGame()
        {
            return new GameDTO
            {
                Id = 1,
                Owner = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Owner" },
                Name = "Carcassonne",
                Price = 25m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 5,
                Description = "Tile placement game about building a medieval landscape.",
                Image = Array.Empty<byte>(),
                IsActive = true,
            };
        }
    }
}
