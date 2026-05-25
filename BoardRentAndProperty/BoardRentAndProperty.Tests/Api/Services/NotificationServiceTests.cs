using System;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Contracts.Models;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using NotificationService = BoardRentAndProperty.Api.Services.NotificationService;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class NotificationServiceTests
    {
        private FakeNotificationRepository notificationRepository = null!;
        private NotificationService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationRepository = new FakeNotificationRepository();
            this.service = new NotificationService(this.notificationRepository, new NotificationMapper(new UserMapper()));
        }

        [Test]
        public void SendNotificationAndDeleteLinkedNotifications_DelegateToRepository()
        {
            var recipientId = Guid.NewGuid();
            this.service.SendNotificationToUser(recipientId, new NotificationDTO
            {
                Recipient = new UserDTO { Id = recipientId, DisplayName = "Receiver" },
                Title = "Hello",
                Body = "World",
                Type = NotificationType.Informational,
                RelatedRequestId = 42,
            });

            Notification savedNotification = this.notificationRepository.LastAddedNotification!;
            Assert.That(this.notificationRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(savedNotification.Title, Is.EqualTo("Hello"));
            Assert.That(savedNotification.RelatedRequest!.Id, Is.EqualTo(42));

            this.service.DeleteNotificationsLinkedToRequest(42);
            Assert.That(this.notificationRepository.DeleteLinkedCallCount, Is.EqualTo(1));
            Assert.That(this.notificationRepository.LastLinkedRequestId, Is.EqualTo(42));
        }
    }
}
