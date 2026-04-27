namespace BoardRent.Tests.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using BoardRent.ViewModels;
    using Moq;
    using Xunit;

    public class RegisterViewModelTests
    {
        private readonly Mock<IAuthService> mockAuthService;
        private readonly RegisterViewModel systemUnderTest;

        public RegisterViewModelTests()
        {
            this.mockAuthService = new Mock<IAuthService>();
            this.systemUnderTest = new RegisterViewModel(this.mockAuthService.Object);
        }

        [Fact]
        public async Task RegisterAsync_SuccessfulRegistration_InvokesSuccessCallback()
        {
            // Arrange
            bool wasNavigationCalled = false;
            this.systemUnderTest.OnRegistrationSuccess = () => wasNavigationCalled = true;

            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            this.systemUnderTest.RegisterCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.True(wasNavigationCalled);
            this.mockAuthService.Verify(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_FieldValidationError_MapsErrorsToCorrectProperties()
        {
            // Arrange
            // Simulăm un format de eroare combinat: Username duplicat și parolă prea scurtă
            string validationErrorMessage = "Username|Username already exists;Password|Password is too short";
            ServiceResult<bool> failResult = ServiceResult<bool>.Fail(validationErrorMessage);

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(failResult);

            // Act
            this.systemUnderTest.RegisterCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal("Username already exists", this.systemUnderTest.UsernameError);
            Assert.Equal("Password is too short", this.systemUnderTest.PasswordError);
            Assert.False(this.systemUnderTest.IsLoading);
        }

        [Fact]
        public async Task RegisterAsync_GeneralError_SetsGeneralErrorMessage()
        {
            // Arrange
            string generalError = "Server connection lost";
            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Fail(generalError));

            // Act
            this.systemUnderTest.RegisterCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal(generalError, this.systemUnderTest.ErrorMessage);
            // Verificăm că erorile specifice pe câmpuri au rămas goale
            Assert.Equal(string.Empty, this.systemUnderTest.EmailError);
        }

        [Fact]
        public void GoToLogin_CommandExecuted_InvokesNavigateBackRequest()
        {
            // Arrange
            bool wasBackNavigationCalled = false;
            this.systemUnderTest.OnNavigateBackRequest = () => wasBackNavigationCalled = true;

            // Act
            this.systemUnderTest.GoToLoginCommand.Execute(null);

            // Assert
            Assert.True(wasBackNavigationCalled);
        }

        [Fact]
        public async Task RegisterAsync_ClearErrors_RemovesOldErrorsBeforeNewAttempt()
        {
            // Arrange
            this.systemUnderTest.UsernameError = "Old error";

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            this.systemUnderTest.RegisterCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal(string.Empty, this.systemUnderTest.UsernameError);
        }
    }
}