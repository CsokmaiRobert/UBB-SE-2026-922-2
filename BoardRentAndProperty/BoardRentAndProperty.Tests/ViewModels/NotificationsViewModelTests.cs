using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class NotificationsViewModelTests
    {
        private readonly Guid currentUserId = Guid.NewGuid();
        private FakeClientNotificationService notificationService = null!;
        private FakeCurrentUserContext currentUserContext = null!;
        private FakeServerClient serverClient = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationService = new FakeClientNotificationService();
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.currentUserId };
            this.serverClient = new FakeServerClient
            {
                ConnectionStatus = NotificationConnectionStatus.Connected,
            };
        }

        [Test]
        public void Constructor_LoadsNotificationsForCurrentUser()
        {
            this.notificationService.NotificationsForUser = ImmutableList.Create(
                    new NotificationDTO { Id = 1, Recipient = new UserDTO { Id = this.currentUserId }, Title = "a", Body = "b" },
                    new NotificationDTO { Id = 2, Recipient = new UserDTO { Id = this.currentUserId }, Title = "c", Body = "d" });

            using var viewModel = new NotificationsViewModel(
                this.notificationService,
                this.currentUserContext,
                this.serverClient);

            Assert.That(viewModel.PagedItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void DeleteNotificationByIdentifier_CallsServiceDelete()
        {
            this.notificationService.NotificationsForUser = ImmutableList<NotificationDTO>.Empty;

            using var viewModel = new NotificationsViewModel(
                this.notificationService,
                this.currentUserContext,
                this.serverClient);

            viewModel.DeleteNotificationByIdentifier(7);

            Assert.That(this.notificationService.DeleteNotificationCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.LastDeletedNotificationId, Is.EqualTo(7));
        }
    }
}
