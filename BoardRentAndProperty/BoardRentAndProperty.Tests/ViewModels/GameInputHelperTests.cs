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
                "Catan",
                19.99m,
                3,
                4,
                "Colonize the island",
                3,
                50,
                0.01m,
                2,
                10,
                200);

            Assert.That(validationErrors, Is.Empty);
        }

        [Test]
        public void BuildValidationErrors_WithLowPriceAndShortDescription_ReturnsPriceAndDescriptionErrors()
        {
            var validationErrors = GameInputHelper.BuildValidationErrors(
                "Saboteur",
                2m,
                2,
                12,
                "Find the gold",
                3,
                50,
                20.01m,
                2,
                100,
                200);

            Assert.That(validationErrors, Does.Contain(ValidationMessages.PriceMinimum(20.01m)));
            Assert.That(validationErrors, Does.Contain(ValidationMessages.DescriptionLengthRange(100, 200)));
        }

        [Test]
        public void BuildValidationErrors_WithEmptyNameAndInvalidPlayerCounts_ReturnsNameAndPlayerCountErrors()
        {
            var validationErrors = GameInputHelper.BuildValidationErrors(
                string.Empty,
                30m,
                11,
                10,
                "Find the gold",
                3,
                50,
                20.01m,
                20,
                1,
                200);

            Assert.That(validationErrors, Does.Contain(ValidationMessages.NameLengthRange(3, 50)));
            Assert.That(validationErrors, Does.Contain(ValidationMessages.MinimumPlayerCount(20)));
            Assert.That(validationErrors, Does.Contain(ValidationMessages.MaximumPlayerCountComparedToMinimum));
        }

        [Test]
        public void EnsureImageOrDefault_WithValidImage_ReturnsSameImage()
        {
            byte[] gameImage = { 1, 2, 3 };

            var result = GameInputHelper.EnsureImageOrDefault(gameImage, "SomeDirectory");

            Assert.That(result, Is.SameAs(gameImage));
        }

        [Test]
        public void EnsureImageOrDefault_WithEmptyImageAndInvalidPath_ReturnsEmptyArray()
        {
            var returnedImage = GameInputHelper.EnsureImageOrDefault(Array.Empty<byte>(), "InvalidDirectory123456789");

            Assert.That(returnedImage, Is.Empty);
        }
    }
}
