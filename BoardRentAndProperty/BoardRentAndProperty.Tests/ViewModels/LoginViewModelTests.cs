using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class LoginViewModelTests
    {
        private Mock<IAuthService> authServiceMock = null!;
        private LoginViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.authServiceMock = new Mock<IAuthService>();
            this.systemUnderTest = new LoginViewModel(this.authServiceMock.Object);
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

            this.authServiceMock
                .Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<AccountProfileDataTransferObject>.Ok(profile));

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Administrator"));
            this.authServiceMock.Verify(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()), Times.Once);
        }

        [Test]
        public async Task LoginAsync_EmptyFields_SetsLocalErrorMessageWithoutCallingService()
        {
            this.systemUnderTest.UsernameOrEmail = string.Empty;
            this.systemUnderTest.Password = string.Empty;

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Please enter both username/email and password."));
            this.authServiceMock.Verify(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()), Times.Never);
        }

        [Test]
        public async Task LoginAsync_ServiceReturnsError_SetsErrorMessage()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "wrongpass";

            string serviceError = "Invalid username or password.";
            this.authServiceMock
                .Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<AccountProfileDataTransferObject>.Fail(serviceError));

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

            this.authServiceMock
                .Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<AccountProfileDataTransferObject>.Ok(profile));

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Standard User"));
        }
    }
}
