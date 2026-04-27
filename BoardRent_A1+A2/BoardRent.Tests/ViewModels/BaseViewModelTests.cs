namespace BoardRent.Tests.ViewModels
{
    using BoardRent.ViewModels;
    using Xunit;

    public class BaseViewModelTests
    {
        [Fact]
        public void IsLoading_PropertyChanged_NotifiesUI()
        {
            // Arrange
            BaseViewModel viewModel = new BaseViewModel();
            bool propertyChangedRaised = false;

            // Ne abonăm la evenimentul de schimbare a proprietății
            viewModel.PropertyChanged += (sender, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BaseViewModel.IsLoading))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            viewModel.IsLoading = true;

            // Assert
            Assert.True(viewModel.IsLoading);
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void ErrorMessage_SetNewValue_UpdatesCorrectly()
        {
            // Arrange
            BaseViewModel viewModel = new BaseViewModel();
            string expectedMessage = "Invalid credentials provided.";

            // Act
            viewModel.ErrorMessage = expectedMessage;

            // Assert
            Assert.Equal(expectedMessage, viewModel.ErrorMessage);
        }

        [Fact]
        public void InitialValues_WhenCreated_AreDefault()
        {
            // Arrange & Act
            BaseViewModel viewModel = new BaseViewModel();

            // Assert
            Assert.False(viewModel.IsLoading);
            Assert.Null(viewModel.ErrorMessage);
        }
    }
}