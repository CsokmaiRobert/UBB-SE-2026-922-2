using System;
using BoardRentAndProperty.Api.Constants;
using BoardRentAndProperty.Api.Services;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class GameInputHelperTests
    {
        [Test]
        public void BuildValidationErrors_WithAllValidInputs_ReturnsEmptyErrorList()
        {
            var validationErrors = GameInputHelper.BuildValidationErrors(
                "Catan", 19.99m, 3, 4, "Colonize the island", 3, 50, 0.01m, 2, 10, 200);

            Assert.That(validationErrors, Is.Empty);
        }

        [Test]
        public void BuildValidationErrors_CollectsAllRelevantErrorsForBrokenInputs()
        {
            var lowPriceShortDescription = GameInputHelper.BuildValidationErrors(
                "Saboteur", 2m, 2, 12, "Find the gold", 3, 50, 20.01m, 2, 100, 200);
            Assert.That(lowPriceShortDescription, Does.Contain(ValidationMessages.PriceMinimum(20.01m)));
            Assert.That(lowPriceShortDescription, Does.Contain(ValidationMessages.DescriptionLengthRange(100, 200)));

            var nameAndPlayerCountErrors = GameInputHelper.BuildValidationErrors(
                string.Empty, 30m, 11, 10, "Find the gold", 3, 50, 20.01m, 20, 1, 200);
            Assert.That(nameAndPlayerCountErrors, Does.Contain(ValidationMessages.NameLengthRange(3, 50)));
            Assert.That(nameAndPlayerCountErrors, Does.Contain(ValidationMessages.MinimumPlayerCount(20)));
            Assert.That(nameAndPlayerCountErrors, Does.Contain(ValidationMessages.MaximumPlayerCountComparedToMinimum));
        }

        [Test]
        public void EnsureImageOrDefault_PassesThroughExistingImageOrReturnsEmpty()
        {
            byte[] gameImage = { 1, 2, 3 };
            var passThroughResult = GameInputHelper.EnsureImageOrDefault(gameImage, "SomeDirectory");
            Assert.That(passThroughResult, Is.SameAs(gameImage));

            var fallbackResult = GameInputHelper.EnsureImageOrDefault(Array.Empty<byte>(), "InvalidDirectory123456789");
            Assert.That(fallbackResult, Is.Empty);
        }
    }
}
