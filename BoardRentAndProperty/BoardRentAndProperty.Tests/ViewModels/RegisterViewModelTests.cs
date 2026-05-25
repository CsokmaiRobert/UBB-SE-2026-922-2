using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class RegisterViewModelTests
    {
        private FakeClientAuthService authService = null!;
        private RegisterViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.authService = new FakeClientAuthService();
            this.systemUnderTest = new RegisterViewModel(this.authService);
        }

        [Test]
        public async Task RegisterAsync_SuccessfulRegistration_InvokesSuccessCallbackAndClearsOldErrors()
        {
            bool registrationSuccessCallbackWasCalled = false;
            this.systemUnderTest.OnRegistrationSuccess = () => registrationSuccessCallbackWasCalled = true;
            this.systemUnderTest.UsernameError = "Old error";
            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";
            this.authService.RegisterResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(registrationSuccessCallbackWasCalled, Is.True);
            Assert.That(this.systemUnderTest.UsernameError, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task RegisterAsync_FieldValidationError_MapsErrorsToCorrectProperties()
        {
            this.authService.RegisterResult =
                ServiceResult<bool>.Fail("Username|Username already exists;Password|Password is too short");

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.UsernameError, Is.EqualTo("Username already exists"));
            Assert.That(this.systemUnderTest.PasswordError, Is.EqualTo("Password is too short"));
        }

        [Test]
        public async Task RegisterAsync_GeneralError_SetsGeneralErrorMessage()
        {
            this.authService.RegisterResult = ServiceResult<bool>.Fail("Server connection lost");

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Server connection lost"));
            Assert.That(this.systemUnderTest.EmailError, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GoToLogin_WhenExecuted_InvokesNavigateBackRequest()
        {
            bool navigateBackWasCalled = false;
            this.systemUnderTest.OnNavigateBackRequest = () => navigateBackWasCalled = true;

            this.systemUnderTest.GoToLoginCommand.Execute(null);

            Assert.That(navigateBackWasCalled, Is.True);
        }
    }
}
