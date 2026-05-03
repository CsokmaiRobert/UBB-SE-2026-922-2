using System;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Contracts.Models;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class NotificationServiceTests
    {
        private Mock<INotificationRepository> notificationRepositoryMock = null!;
        private NotificationService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationRepositoryMock = new Mock<INotificationRepository>();
            this.service = new NotificationService(this.notificationRepositoryMock.Object, new NotificationMapper(new UserMapper()));
        }

        [Test]
        public void SendNotificationToUser_SavesNotificationInRepository()
        {
            var recipientId = Guid.NewGuid();
            var notification = new NotificationDTO
            {
                Recipient = new UserDTO { Id = recipientId, DisplayName = "Receiver" },
                Title = "Hello",
                Body = "World",
                Type = NotificationType.Informational,
                RelatedRequestId = 42,
            };

            this.service.SendNotificationToUser(recipientId, notification);

            this.notificationRepositoryMock.Verify(repository => repository.Add(It.Is<Notification>(savedNotification =>
                savedNotification.Recipient != null
                && savedNotification.Recipient.Id == recipientId
                && savedNotification.Title == "Hello"
                && savedNotification.Body == "World"
                && savedNotification.RelatedRequest != null
                && savedNotification.RelatedRequest.Id == 42)), Times.Once);
        }

        [Test]
        public void DeleteNotificationsLinkedToRequest_CallsRepository()
        {
            this.service.DeleteNotificationsLinkedToRequest(42);

            this.notificationRepositoryMock.Verify(repository => repository.DeleteNotificationsLinkedToRequest(42), Times.Once);
        }
    }
}
