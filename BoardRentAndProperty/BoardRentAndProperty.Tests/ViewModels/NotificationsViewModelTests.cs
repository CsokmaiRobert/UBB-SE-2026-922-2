using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class NotificationsViewModelTests
    {
        private readonly Guid currentUserId = Guid.NewGuid();
        private Mock<INotificationService> notificationServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;
        private Mock<IServerClient> serverClientMock = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationServiceMock = new Mock<INotificationService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();
            this.serverClientMock = new Mock<IServerClient>();

            this.currentUserContextMock.SetupGet(context => context.CurrentUserId).Returns(this.currentUserId);
            this.notificationServiceMock
                .Setup(service => service.Subscribe(It.IsAny<IObserver<NotificationDTO>>()))
                .Returns(Mock.Of<IDisposable>());
            this.serverClientMock
                .SetupGet(client => client.ConnectionStatus)
                .Returns(NotificationConnectionStatus.Connected);
        }

        [Test]
        public void Constructor_LoadsNotificationsForCurrentUser()
        {
            this.notificationServiceMock
                .Setup(service => service.GetNotificationsForUser(this.currentUserId))
                .Returns(ImmutableList.Create(
                    new NotificationDTO { Id = 1, Recipient = new UserDTO { Id = this.currentUserId }, Title = "a", Body = "b" },
                    new NotificationDTO { Id = 2, Recipient = new UserDTO { Id = this.currentUserId }, Title = "c", Body = "d" }));

            using var viewModel = new NotificationsViewModel(
                this.notificationServiceMock.Object,
                this.currentUserContextMock.Object,
                this.serverClientMock.Object);

            Assert.That(viewModel.PagedItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void DeleteNotificationByIdentifier_CallsServiceDelete()
        {
            this.notificationServiceMock
                .Setup(service => service.GetNotificationsForUser(this.currentUserId))
                .Returns(ImmutableList<NotificationDTO>.Empty);

            using var viewModel = new NotificationsViewModel(
                this.notificationServiceMock.Object,
                this.currentUserContextMock.Object,
                this.serverClientMock.Object);

            viewModel.DeleteNotificationByIdentifier(7);

            this.notificationServiceMock.Verify(service => service.DeleteNotificationByIdentifier(7), Times.Once);
        }
    }
}
