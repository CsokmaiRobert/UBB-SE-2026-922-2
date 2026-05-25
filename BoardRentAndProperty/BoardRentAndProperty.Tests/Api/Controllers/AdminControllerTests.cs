using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using ApiServiceResult = BoardRentAndProperty.Api.Utilities.ServiceResult<bool>;
using ApiAccountListResult = BoardRentAndProperty.Api.Utilities.ServiceResult<System.Collections.Generic.List<BoardRentAndProperty.Contracts.DataTransferObjects.AccountProfileDataTransferObject>>;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class AdminControllerTests
    {
        private FakeAdminService adminService = null!;
        private AdminController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.adminService = new FakeAdminService();
            this.controller = new AdminController(this.adminService);
        }

        [Test]
        public async Task GetAccounts_WithDefaultPaging_ForwardsDefaultsAndReturnsOk()
        {
            var profiles = new List<AccountProfileDataTransferObject>
            {
                new AccountProfileDataTransferObject { Username = "first" },
            };
            this.adminService.GetAllOutcome = ApiAccountListResult.Ok(profiles);

            var controllerResponse = await this.controller.GetAccounts();
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.SameAs(profiles));
            Assert.That(this.adminService.LastPageNumber, Is.EqualTo(1));
            Assert.That(this.adminService.LastPageSize, Is.EqualTo(100));
        }

        [Test]
        public async Task GetAccounts_WhenServiceFails_ReturnsErrorObjectResult()
        {
            this.adminService.GetAllOutcome = ApiAccountListResult.Fail("Could not load accounts.");

            var controllerResponse = await this.controller.GetAccounts();
            var errorResult = controllerResponse.Result as ObjectResult;

            Assert.That(errorResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task SuspendUnsuspendUnlock_OnSuccess_ReturnNoContentAndPassAccountIdThrough()
        {
            var accountId = Guid.NewGuid();

            Assert.That(await this.controller.Suspend(accountId), Is.InstanceOf<NoContentResult>());
            Assert.That(this.adminService.LastTargetAccountId, Is.EqualTo(accountId));

            Assert.That(await this.controller.Unsuspend(accountId), Is.InstanceOf<NoContentResult>());
            Assert.That(await this.controller.Unlock(accountId), Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Suspend_WhenAccountNotFound_ReturnsNotFoundObjectResult()
        {
            this.adminService.SuspendOutcome = ApiServiceResult.Fail("Account not found.");

            var controllerResponse = await this.controller.Suspend(Guid.NewGuid());

            Assert.That(((ObjectResult)controllerResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ResetPassword_WhenServiceSucceeds_ReturnsNoContentAndForwardsPassword()
        {
            var accountId = Guid.NewGuid();
            var resetBody = new ResetPasswordDataTransferObject { NewPassword = "BrandNew!1" };

            var controllerResponse = await this.controller.ResetPassword(accountId, resetBody);

            Assert.That(controllerResponse, Is.InstanceOf<NoContentResult>());
            Assert.That(this.adminService.LastResetPasswordValue, Is.EqualTo("BrandNew!1"));
        }
    }
}
