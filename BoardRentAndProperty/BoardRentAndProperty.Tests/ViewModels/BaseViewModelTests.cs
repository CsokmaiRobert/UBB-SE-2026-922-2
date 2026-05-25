using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class BaseViewModelTests
    {
        [Test]
        public void Constructor_DefaultsAndPropertyChanges_BehaveAsExpected()
        {
            var viewModel = new BaseViewModel();
            Assert.That(viewModel.IsLoading, Is.False);
            Assert.That(viewModel.ErrorMessage, Is.Null);

            bool isLoadingChangedRaised = false;
            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BaseViewModel.IsLoading))
                {
                    isLoadingChangedRaised = true;
                }
            };
            viewModel.IsLoading = true;
            Assert.That(viewModel.IsLoading, Is.True);
            Assert.That(isLoadingChangedRaised, Is.True);

            viewModel.ErrorMessage = "Invalid credentials provided.";
            Assert.That(viewModel.ErrorMessage, Is.EqualTo("Invalid credentials provided."));
        }
    }
}
