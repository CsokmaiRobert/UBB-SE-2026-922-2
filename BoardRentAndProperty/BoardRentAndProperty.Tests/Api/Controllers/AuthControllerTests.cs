using System.Net;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using ApiServiceResult = BoardRentAndProperty.Api.Utilities.ServiceResult<bool>;
using ApiProfileResult = BoardRentAndProperty.Api.Utilities.ServiceResult<BoardRentAndProperty.Contracts.DataTransferObjects.AccountProfileDataTransferObject>;
using ApiStringResult = BoardRentAndProperty.Api.Utilities.ServiceResult<string>;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class AuthControllerTests
    {
        private FakeAuthService authService = null!;
        private AuthController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.authService = new FakeAuthService();
            this.controller = new AuthController(this.authService);
        }

        [Test]
        public async Task Register_WhenServiceSucceeds_ReturnsOkAndPassesPayloadThrough()
        {
            var registrationBody = new RegisterDataTransferObject { Username = "new_user" };

            var controllerResponse = await this.controller.Register(registrationBody);

            Assert.That(controllerResponse, Is.InstanceOf<OkObjectResult>());
            Assert.That(this.authService.LastRegisterPayload, Is.SameAs(registrationBody));
        }

        [Test]
        public async Task Register_WhenServiceReportsAlreadyTakenError_ReturnsConflictObjectResult()
        {
            this.authService.RegisterOutcome = ApiServiceResult.Fail("Username already taken.");

            var controllerResponse = await this.controller.Register(new RegisterDataTransferObject());

            Assert.That(((ObjectResult)controllerResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));
        }

        [Test]
        public async Task Login_WhenServiceSucceeds_ReturnsOkWithProfileData()
        {
            var profile = new AccountProfileDataTransferObject { Username = "user1" };
            this.authService.LoginOutcome = ApiProfileResult.Ok(profile);

            var controllerResponse = await this.controller.Login(new LoginDataTransferObject());
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.SameAs(profile));
        }

        [Test]
        public async Task Login_ReturnsCorrectStatusForInvalidCredentialsAndSuspendedAccount()
        {
            this.authService.LoginOutcome = ApiProfileResult.Fail("Invalid username or password.");
            var unauthorizedResponse = await this.controller.Login(new LoginDataTransferObject());
            Assert.That(((ObjectResult)unauthorizedResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));

            this.authService.LoginOutcome = ApiProfileResult.Fail("Your account has been suspended.");
            var forbiddenResponse = await this.controller.Login(new LoginDataTransferObject());
            Assert.That(((ObjectResult)forbiddenResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task Logout_WhenServiceSucceeds_ReturnsNoContent()
        {
            var controllerResponse = await this.controller.Logout();

            Assert.That(controllerResponse, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task ForgotPassword_WhenServiceSucceeds_ReturnsOkWithReturnedString()
        {
            this.authService.ForgotPasswordOutcome = ApiStringResult.Ok("reset-link");

            var controllerResponse = await this.controller.ForgotPassword();
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.EqualTo("reset-link"));
        }
    }
}
