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
        public async Task RegisterAsync_SuccessfulRegistration_InvokesSuccessCallback()
        {
            bool registrationSuccessCallbackWasCalled = false;
            this.systemUnderTest.OnRegistrationSuccess = () => registrationSuccessCallbackWasCalled = true;
            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";

            this.authService.RegisterResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(registrationSuccessCallbackWasCalled, Is.True);
            Assert.That(this.authService.RegisterCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task RegisterAsync_FieldValidationError_MapsErrorsToCorrectProperties()
        {
            string validationError = "Username|Username already exists;Password|Password is too short";

            this.authService.RegisterResult = ServiceResult<bool>.Fail(validationError);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.UsernameError, Is.EqualTo("Username already exists"));
            Assert.That(this.systemUnderTest.PasswordError, Is.EqualTo("Password is too short"));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public async Task RegisterAsync_GeneralError_SetsGeneralErrorMessage()
        {
            string generalError = "Server connection lost";

            this.authService.RegisterResult = ServiceResult<bool>.Fail(generalError);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(generalError));
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

        [Test]
        public async Task RegisterAsync_ClearsOldErrorsBeforeNewAttempt()
        {
            this.systemUnderTest.UsernameError = "Old error";

            this.authService.RegisterResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.UsernameError, Is.EqualTo(string.Empty));
        }
    }
}
