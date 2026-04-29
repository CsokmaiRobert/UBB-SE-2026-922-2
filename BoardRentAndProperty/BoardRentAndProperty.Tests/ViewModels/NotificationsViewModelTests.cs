using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public class NotificationsViewModelTests
    {
        private Mock<INotificationService> notificationService = null!;
        private Mock<ICurrentUserContext> currentUserContext = null!;
        private int currentUserId;

        [SetUp]
        public void SetUp()
        {
            notificationService = new Mock<INotificationService>();
            currentUserContext = new Mock<ICurrentUserContext>();
            currentUserId = 2;

            currentUserContext
                .SetupGet(context => context.CurrentUserId)
                .Returns(() => currentUserId);
            notificationService
                .Setup(service => service.Subscribe(It.IsAny<IObserver<NotificationDTO>>()))
                .Returns(Mock.Of<IDisposable>());
        }

        [Test]
        public void ConstructorLoadsCurrentUsersNotificationsNewestFirst()
        {
            notificationService
                .Setup(service => service.GetNotificationsForUser(2))
                .Returns(ImmutableList.Create(
                    BuildNotification(1, 2, DateTime.UtcNow.AddMinutes(-20)),
                    BuildNotification(2, 2, DateTime.UtcNow),
                    BuildNotification(3, 2, DateTime.UtcNow.AddMinutes(-10))));

            var viewModel = new NotificationsViewModel(notificationService.Object, currentUserContext.Object);

            Assert.That(viewModel.PagedItems.Select(notification => notification.Id), Is.EqualTo(new[] { 2, 3, 1 }));
        }

        [Test]
        public void ConstructorDoesNotLoadRepositoryWhenNoUserIsLoggedIn()
        {
            currentUserId = 0;

            var viewModel = new NotificationsViewModel(notificationService.Object, currentUserContext.Object);

            Assert.That(viewModel.PagedItems, Is.Empty);
            notificationService.Verify(service => service.GetNotificationsForUser(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void OnNextIgnoresNotificationsForOtherUsers()
        {
            notificationService
                .Setup(service => service.GetNotificationsForUser(2))
                .Returns(ImmutableList<NotificationDTO>.Empty);
            var viewModel = new NotificationsViewModel(notificationService.Object, currentUserContext.Object);
            notificationService.Invocations.Clear();

            viewModel.OnNext(BuildNotification(9, 3, DateTime.UtcNow));

            notificationService.Verify(service => service.GetNotificationsForUser(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void OnNextReloadsCurrentUserNotifications()
        {
            notificationService
                .Setup(service => service.GetNotificationsForUser(2))
                .Returns(ImmutableList<NotificationDTO>.Empty);
            var viewModel = new NotificationsViewModel(notificationService.Object, currentUserContext.Object);
            notificationService.Invocations.Clear();

            viewModel.OnNext(BuildNotification(9, 2, DateTime.UtcNow));

            notificationService.Verify(service => service.GetNotificationsForUser(2), Times.Once);
        }

        private static NotificationDTO BuildNotification(int id, int userId, DateTime timestamp)
        {
            return new NotificationDTO
            {
                Id = id,
                User = new UserDTO { Id = userId },
                Timestamp = timestamp,
                Title = "Title",
                Body = "Body"
            };
        }
    }
}
