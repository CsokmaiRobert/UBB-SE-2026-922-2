using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class NotificationsControllerTests
    {
        private FakeNotificationServiceForController notificationService = null!;
        private NotificationsController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationService = new FakeNotificationServiceForController();
            this.controller = new NotificationsController(this.notificationService);
        }

        [Test]
        public void GetForUser_ReturnsOkWithUserNotifications()
        {
            this.notificationService.UserNotifications = ImmutableList.Create(new NotificationDTO { Id = 1, Title = "Hello" });

            var controllerResponse = this.controller.GetForUser(Guid.NewGuid());
            var okResult = controllerResponse.Result as OkObjectResult;
            var returnedNotifications = okResult!.Value as ImmutableList<NotificationDTO>;

            Assert.That(returnedNotifications![0].Title, Is.EqualTo("Hello"));
        }

        [Test]
        public void GetById_HandlesSuccessAndKeyNotFound()
        {
            var expectedNotification = new NotificationDTO { Id = 42 };
            this.notificationService.NotificationsById[42] = expectedNotification;
            var successResponse = this.controller.GetById(42);
            Assert.That(((OkObjectResult)successResponse.Result!).Value, Is.SameAs(expectedNotification));

            this.notificationService.GetByIdentifierException = new KeyNotFoundException();
            var notFoundResponse = this.controller.GetById(99);
            Assert.That(((ObjectResult)notFoundResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public void Update_HandlesSuccessAndKeyNotFound()
        {
            var noContentResponse = this.controller.Update(5, new NotificationDTO());
            Assert.That(noContentResponse, Is.InstanceOf<NoContentResult>());

            this.notificationService.UpdateException = new KeyNotFoundException();
            var notFoundResponse = this.controller.Update(5, new NotificationDTO());
            Assert.That(((ObjectResult)notFoundResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public void Delete_HandlesSuccessAndKeyNotFound()
        {
            var deletedNotification = new NotificationDTO { Id = 7 };
            this.notificationService.NotificationsById[7] = deletedNotification;
            var okResponse = this.controller.Delete(7);
            Assert.That(((OkObjectResult)okResponse.Result!).Value, Is.SameAs(deletedNotification));

            this.notificationService.DeleteException = new KeyNotFoundException();
            var notFoundResponse = this.controller.Delete(404);
            Assert.That(((ObjectResult)notFoundResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }
    }
}
