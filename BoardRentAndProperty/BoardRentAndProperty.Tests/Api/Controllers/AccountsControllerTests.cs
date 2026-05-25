using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using ApiServiceResult = BoardRentAndProperty.Api.Utilities.ServiceResult<bool>;
using ApiProfileResult = BoardRentAndProperty.Api.Utilities.ServiceResult<BoardRentAndProperty.Contracts.DataTransferObjects.AccountProfileDataTransferObject>;
using ApiStringResult = BoardRentAndProperty.Api.Utilities.ServiceResult<string>;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class AccountsControllerTests
    {
        private FakeAccountService accountService = null!;
        private FakeAvatarStorageService avatarStorageService = null!;
        private AccountsController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.accountService = new FakeAccountService();
            this.avatarStorageService = new FakeAvatarStorageService();
            this.controller = new AccountsController(this.accountService, this.avatarStorageService);
        }

        [Test]
        public async Task GetProfile_WhenServiceSucceeds_ReturnsOkWithProfile()
        {
            var accountId = Guid.NewGuid();
            var profile = new AccountProfileDataTransferObject { Id = accountId, Username = "owner" };
            this.accountService.GetProfileOutcome = ApiProfileResult.Ok(profile);

            var controllerResponse = await this.controller.GetProfile(accountId);
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.SameAs(profile));
        }

        [Test]
        public async Task GetProfile_WhenAccountIsMissing_ReturnsNotFound()
        {
            this.accountService.GetProfileOutcome = ApiProfileResult.Fail("Account not found.");

            var controllerResponse = await this.controller.GetProfile(Guid.NewGuid());
            var notFoundResult = controllerResponse.Result as ObjectResult;

            Assert.That(notFoundResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateProfile_ChangePassword_RemoveAvatar_ReturnNoContentOnSuccess()
        {
            var noContentForUpdate = await this.controller.UpdateProfile(Guid.NewGuid(), new AccountProfileDataTransferObject());
            Assert.That(noContentForUpdate, Is.InstanceOf<NoContentResult>());

            var noContentForChangePassword = await this.controller.ChangePassword(Guid.NewGuid(), new ChangePasswordDataTransferObject());
            Assert.That(noContentForChangePassword, Is.InstanceOf<NoContentResult>());

            var noContentForRemoveAvatar = await this.controller.RemoveAvatar(Guid.NewGuid());
            Assert.That(noContentForRemoveAvatar, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task ChangePassword_WhenCurrentPasswordIsWrong_ReturnsUnauthorized()
        {
            this.accountService.ChangePasswordOutcome = ApiServiceResult.Fail("Current password is incorrect.");

            var controllerResponse = await this.controller.ChangePassword(Guid.NewGuid(), new ChangePasswordDataTransferObject());
            var unauthorizedResult = controllerResponse as ObjectResult;

            Assert.That(unauthorizedResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task UploadAvatar_WithMissingOrEmptyFile_ReturnsValidationError()
        {
            var nullFileResponse = await this.controller.UploadAvatar(Guid.NewGuid(), null!);
            Assert.That(((ObjectResult)nullFileResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            var emptyFile = new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, "file", "empty.png");
            var emptyFileResponse = await this.controller.UploadAvatar(Guid.NewGuid(), emptyFile);
            Assert.That(((ObjectResult)emptyFileResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UploadAvatar_WhenAccountUpdateFails_DeletesStoredFileAndReturnsError()
        {
            byte[] avatarBytes = new byte[] { 1 };
            var validFile = new FormFile(new MemoryStream(avatarBytes), 0, avatarBytes.Length, "file", "avatar.png");
            this.avatarStorageService.SavedPath = "/avatars/abc.png";
            this.accountService.SetAvatarUrlOutcome = ApiStringResult.Fail("Account not found.");

            var controllerResponse = await this.controller.UploadAvatar(Guid.NewGuid(), validFile);

            var errorResult = controllerResponse.Result as ObjectResult;
            Assert.That(errorResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
            Assert.That(this.avatarStorageService.DeleteCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task UploadAvatar_WithValidFileAndSuccessfulPersistence_ReturnsOkWithAvatarUrl()
        {
            byte[] avatarBytes = new byte[] { 11, 22, 33 };
            var validFile = new FormFile(new MemoryStream(avatarBytes), 0, avatarBytes.Length, "file", "avatar.png");
            this.avatarStorageService.SavedPath = "/avatars/abc.png";
            this.accountService.SetAvatarUrlOutcome = ApiStringResult.Ok("/avatars/abc.png");

            var controllerResponse = await this.controller.UploadAvatar(Guid.NewGuid(), validFile);

            var okResult = controllerResponse.Result as OkObjectResult;
            var responsePayload = okResult!.Value as AvatarUploadResponseDataTransferObject;
            Assert.That(responsePayload!.AvatarUrl, Is.EqualTo("/avatars/abc.png"));
        }
    }
}
