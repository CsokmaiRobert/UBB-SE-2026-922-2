using System;
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
            this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(
                new AccountProfileDataTransferObject
                {
                    Username = "admin",
                    Role = new RoleDataTransferObject { Name = "Administrator" },
                });

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Administrator"));
            Assert.That(this.authService.LoginCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoginAsync_NullRole_DefaultsToStandardUser()
        {
            string capturedRole = string.Empty;
            this.systemUnderTest.OnLoginSuccess = role => capturedRole = role;
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "pass";
            this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(
                new AccountProfileDataTransferObject { Username = "user", Role = null! });

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Standard User"));
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
        public async Task LoginAsync_ServiceFailureOrSuccessCallbackThrowing_SurfacesErrorMessage()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "wrongpass";
            this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Fail("Invalid username or password.");
            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);
            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Invalid username or password."));

            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "ValidPassword123!";
            this.systemUnderTest.OnLoginSuccess = _ => throw new InvalidOperationException("Navigation failed.");
            this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(
                new AccountProfileDataTransferObject
                {
                    Username = "user",
                    Role = new RoleDataTransferObject { Name = "Standard User" },
                });
            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);
            Assert.That(this.systemUnderTest.ErrorMessage, Does.Contain("Navigation failed."));
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
    }
}
