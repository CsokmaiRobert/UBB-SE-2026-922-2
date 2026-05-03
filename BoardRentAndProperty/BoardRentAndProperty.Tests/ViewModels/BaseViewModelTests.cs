using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class BaseViewModelTests
    {
        [Test]
        public void IsLoading_WhenValueChanges_RaisesPropertyChanged()
        {
            var viewModel = new BaseViewModel();
            bool propertyChangedRaised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BaseViewModel.IsLoading))
                {
                    propertyChangedRaised = true;
                }
            };

            viewModel.IsLoading = true;

            Assert.That(viewModel.IsLoading, Is.True);
            Assert.That(propertyChangedRaised, Is.True);
        }

        [Test]
        public void ErrorMessage_WhenAssigned_StoresTheNewValue()
        {
            var viewModel = new BaseViewModel();
            string expectedMessage = "Invalid credentials provided.";

            viewModel.ErrorMessage = expectedMessage;

            Assert.That(viewModel.ErrorMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Constructor_WhenCreated_UsesDefaultValues()
        {
            var viewModel = new BaseViewModel();

            Assert.That(viewModel.IsLoading, Is.False);
            Assert.That(viewModel.ErrorMessage, Is.Null);
        }
    }
}
