using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class UsersControllerTests
    {
        private FakeUserService userService = null!;
        private UsersController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.userService = new FakeUserService();
            this.controller = new UsersController(this.userService);
        }

        [Test]
        public void GetUsersExcept_ReturnsOkWithFilteredUsersFromService()
        {
            var excludedAccountId = Guid.NewGuid();
            var availableUser = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Visible" };
            this.userService.AvailableUsers = ImmutableList.Create(availableUser);

            var controllerResponse = this.controller.GetUsersExcept(excludedAccountId);
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            Assert.That(this.userService.LastExcludedAccountId, Is.EqualTo(excludedAccountId));
            var returnedUsers = okResult!.Value as ImmutableList<UserDTO>;
            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!, Has.Count.EqualTo(1));
            Assert.That(returnedUsers[0].DisplayName, Is.EqualTo("Visible"));
        }

        [Test]
        public void GetUsersExcept_WithNoUsersAvailable_ReturnsEmptyList()
        {
            var excludedAccountId = Guid.NewGuid();

            var controllerResponse = this.controller.GetUsersExcept(excludedAccountId);
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            var returnedUsers = okResult!.Value as ImmutableList<UserDTO>;
            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!, Is.Empty);
        }
    }
}
