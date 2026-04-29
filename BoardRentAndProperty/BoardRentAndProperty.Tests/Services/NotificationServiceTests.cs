using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Services
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> notificationRepository = null!;
        private Mock<IMapper<Notification, NotificationDTO>> notificationMapper = null!;
        private Mock<IServerClient> serverClient = null!;
        private Mock<ICurrentUserContext> currentUserContext = null!;
        private Mock<IToastNotificationService> toastNotificationService = null!;
        private NotificationService notificationService = null!;

        [SetUp]
        public void SetUp()
        {
            notificationRepository = new Mock<INotificationRepository>();
            notificationMapper = new Mock<IMapper<Notification, NotificationDTO>>();
            serverClient = new Mock<IServerClient>();
            currentUserContext = new Mock<ICurrentUserContext>();
            toastNotificationService = new Mock<IToastNotificationService>();

            currentUserContext.SetupGet(context => context.CurrentUserId).Returns(1);
            serverClient
                .Setup(client => client.Subscribe(It.IsAny<IObserver<IncomingNotification>>()))
                .Returns(Mock.Of<IDisposable>());

            notificationService = new NotificationService(
                notificationRepository.Object,
                notificationMapper.Object,
                serverClient.Object,
                currentUserContext.Object,
                toastNotificationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            notificationService.Dispose();
        }

        [Test]
        public void SendNotificationToUserPersistsAndPushesServerMessage()
        {
            var notification = new NotificationDTO
            {
                Title = "Request approved",
                Body = "Your rental request was approved.",
                Type = NotificationType.Informational,
                RelatedRequestId = 42
            };

            notificationService.SendNotificationToUser(2, notification);

            notificationRepository.Verify(
                repository => repository.Add(It.Is<Notification>(persistedNotification =>
                    persistedNotification.User.Id == 2
                    && persistedNotification.Title == notification.Title
                    && persistedNotification.Body == notification.Body
                    && persistedNotification.RelatedRequestId == 42)),
                Times.Once);
            serverClient.Verify(
                client => client.SendNotification(2, notification.Title, notification.Body),
                Times.Once);
        }

        [Test]
        public void SendNotificationToCurrentUserBroadcastsPersistedNotification()
        {
            const int persistedNotificationId = 21;
            currentUserContext.SetupGet(context => context.CurrentUserId).Returns(2);
            notificationRepository
                .Setup(repository => repository.Add(It.IsAny<Notification>()))
                .Callback<Notification>(persistedNotification => persistedNotification.Id = persistedNotificationId);
            var observer = new Mock<IObserver<NotificationDTO>>();

            using var subscription = notificationService.Subscribe(observer.Object);

            notificationService.SendNotificationToUser(
                2,
                new NotificationDTO
                {
                    Title = "New notification",
                    Body = "Body"
                });

            observer.Verify(
                subscriber => subscriber.OnNext(It.Is<NotificationDTO>(notification =>
                    notification.Id == persistedNotificationId
                    && notification.User.Id == 2
                    && notification.Title == "New notification")),
                Times.Once);
        }

        [Test]
        public void IncomingNotificationBroadcastsAndShowsToast()
        {
            var observer = new Mock<IObserver<NotificationDTO>>();
            using var subscription = notificationService.Subscribe(observer.Object);
            var incomingNotification = new IncomingNotification
            {
                UserId = 1,
                Timestamp = DateTime.UtcNow,
                Title = "Live update",
                Body = "A notification arrived."
            };

            notificationService.OnNext(incomingNotification);

            observer.Verify(
                subscriber => subscriber.OnNext(It.Is<NotificationDTO>(notification =>
                    notification.User.Id == incomingNotification.UserId
                    && notification.Title == incomingNotification.Title
                    && notification.Body == incomingNotification.Body)),
                Times.Once);
            toastNotificationService.Verify(
                toastService => toastService.Show(incomingNotification.Title, incomingNotification.Body),
                Times.Once);
        }

        [Test]
        public async Task CancelUpcomingRentalReminderPreventsScheduledNotifications()
        {
            var rentalStartDate = DateTime.UtcNow.AddHours(24).AddMilliseconds(300);

            notificationService.ScheduleUpcomingRentalReminder(
                100,
                1,
                2,
                "Catan",
                rentalStartDate);
            notificationService.CancelUpcomingRentalReminder(100);

            await Task.Delay(800);

            notificationRepository.Verify(repository => repository.Add(It.IsAny<Notification>()), Times.Never);
            serverClient.Verify(
                client => client.SendNotification(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void GetNotificationsForUserReturnsNewestFirst()
        {
            var olderNotification = new Notification { Id = 1, Timestamp = DateTime.UtcNow.AddMinutes(-10) };
            var newestNotification = new Notification { Id = 2, Timestamp = DateTime.UtcNow };
            var sameTimeHigherIdNotification = new Notification { Id = 3, Timestamp = olderNotification.Timestamp };
            notificationRepository
                .Setup(repository => repository.GetNotificationsByUser(1))
                .Returns(ImmutableList.Create(olderNotification, newestNotification, sameTimeHigherIdNotification));
            notificationMapper
                .Setup(mapper => mapper.ToDTO(It.IsAny<Notification>()))
                .Returns<Notification>(notification => new NotificationDTO
                {
                    Id = notification.Id,
                    Timestamp = notification.Timestamp,
                    User = new UserDTO { Id = 1 }
                });

            var notifications = notificationService.GetNotificationsForUser(1);

            Assert.That(notifications.Select(notification => notification.Id), Is.EqualTo(new[] { 2, 3, 1 }));
        }
    }
}
