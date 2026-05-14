using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class LoginViewModelTests
    {
        private FakeClientAuthService authService = null!;
        private LoginViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.authService = new FakeClientAuthService();
            this.systemUnderTest = new LoginViewModel(this.authService);
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_InvokesSuccessCallbackWithRole()
        {
            string capturedRole = string.Empty;
            this.systemUnderTest.OnLoginSuccess = role => capturedRole = role;
            this.systemUnderTest.UsernameOrEmail = "admin";
            this.systemUnderTest.Password = "Password123!";

            var profile = new AccountProfileDataTransferObject
            {
                Username = "admin",
                Role = new RoleDataTransferObject { Name = "Administrator" },
            };

            this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(profile);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Administrator"));
            Assert.That(this.authService.LoginCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoginAsync_EmptyFields_SetsLocalErrorMessageWithoutCallingService()
        {
            this.systemUnderTest.UsernameOrEmail = string.Empty;
            this.systemUnderTest.Password = string.Empty;

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Please enter both username/email and password."));
            Assert.That(this.authService.LoginCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task LoginAsync_ServiceReturnsError_SetsErrorMessage()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "wrongpass";

            string serviceError = "Invalid username or password.";
            this.authService.LoginResult =
                ServiceResult<AccountProfileDataTransferObject>.Fail(serviceError);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(serviceError));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void NavigateToRegister_WhenExecuted_InvokesCallback()
        {
            bool navigationWasCalled = false;
            this.systemUnderTest.OnNavigateToRegister = () => navigationWasCalled = true;

            this.systemUnderTest.NavigateToRegisterCommand.Execute(null);

            Assert.That(navigationWasCalled, Is.True);
        }

        [Test]
        public async Task LoginAsync_NullRole_DefaultsToStandardUser()
        {
            string capturedRole = string.Empty;
            this.systemUnderTest.OnLoginSuccess = role => capturedRole = role;
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "pass";

            var profile = new AccountProfileDataTransferObject
            {
                Username = "user",
                Role = null!,
            };

            this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(profile);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Standard User"));
        }
    }
}
