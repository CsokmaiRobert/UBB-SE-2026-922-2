using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class ViewOperationResultTests
    {
        [Test]
        public void Success_WithAndWithoutDialogStrings_ProducesSuccessfulResult()
        {
            var emptyDialogResult = ViewOperationResult.Success();
            Assert.That(emptyDialogResult.IsSuccess, Is.True);
            Assert.That(emptyDialogResult.DialogTitle, Is.Empty);

            var explicitDialogResult = ViewOperationResult.Success("Saved", "Game has been saved.");
            Assert.That(explicitDialogResult.IsSuccess, Is.True);
            Assert.That(explicitDialogResult.DialogTitle, Is.EqualTo("Saved"));
            Assert.That(explicitDialogResult.DialogMessage, Is.EqualTo("Game has been saved."));
        }

        [Test]
        public void Failure_WithExplicitOrNullStrings_StoresValuesOrFallsBackToEmpty()
        {
            var explicitFailure = ViewOperationResult.Failure("Error", "Could not contact the server.");
            Assert.That(explicitFailure.IsSuccess, Is.False);
            Assert.That(explicitFailure.DialogMessage, Is.EqualTo("Could not contact the server."));

            var nullFailure = ViewOperationResult.Failure(null!, null!);
            Assert.That(nullFailure.DialogTitle, Is.Empty);
            Assert.That(nullFailure.DialogMessage, Is.Empty);
        }
    }
}
