using System;
using System.Linq;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Repository
{
    [TestFixture]
    [Category("Integration")]
    public sealed class IntegrationNotificationRepositoryTests : DataBaseTests
    {
        private NotificationRepository notificationRepository = null!;
        private RequestRepository requestRepository = null!;

        [SetUp]
        public void SetUp()
        {
            this.notificationRepository = new NotificationRepository(this.DbContextFactory);
            this.requestRepository = new RequestRepository(this.DbContextFactory);
        }

        [Test]
        public void DeleteNotificationsLinkedToRequest_RemovesOnlyMatchingNotifications()
        {
            int gameId = this.SeedGame(OwnerAccountId, "Notification Cleanup Game");
            var targetRequest = BuildRequest(gameId, 80);
            var otherRequest = BuildRequest(gameId, 90);

            this.requestRepository.Add(targetRequest);
            this.requestRepository.Add(otherRequest);

            var firstMatchingNotification = BuildNotification("Match One", targetRequest.Id);
            var secondMatchingNotification = BuildNotification("Match Two", targetRequest.Id);
            var otherLinkedNotification = BuildNotification("Other Link", otherRequest.Id);
            var unlinkedNotification = BuildNotification("No Link");

            this.notificationRepository.Add(firstMatchingNotification);
            this.notificationRepository.Add(secondMatchingNotification);
            this.notificationRepository.Add(otherLinkedNotification);
            this.notificationRepository.Add(unlinkedNotification);

            this.notificationRepository.DeleteNotificationsLinkedToRequest(targetRequest.Id);

            using var dbContext = this.DbContextFactory.CreateDbContext();
            var remainingNotifications = dbContext.Notifications
                .OrderBy(notification => notification.Id)
                .Select(notification => new
                {
                    notification.Id,
                    RelatedRequestId = EF.Property<int?>(notification, "related_request_id"),
                })
                .ToList();

            Assert.That(remainingNotifications.Select(notification => notification.Id), Has.No.Member(firstMatchingNotification.Id));
            Assert.That(remainingNotifications.Select(notification => notification.Id), Has.No.Member(secondMatchingNotification.Id));
            Assert.That(remainingNotifications.Select(notification => notification.Id), Has.Member(otherLinkedNotification.Id));
            Assert.That(remainingNotifications.Select(notification => notification.Id), Has.Member(unlinkedNotification.Id));
            Assert.That(
                remainingNotifications.Single(notification => notification.Id == otherLinkedNotification.Id).RelatedRequestId,
                Is.EqualTo(otherRequest.Id));
            Assert.That(
                remainingNotifications.Single(notification => notification.Id == unlinkedNotification.Id).RelatedRequestId,
                Is.Null);
        }

        private static Request BuildRequest(int gameId, int startOffsetInDays)
        {
            DateTime startDate = new DateTime(2035, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(startOffsetInDays);

            return new Request(
                0,
                new Game { Id = gameId },
                new Account { Id = RenterAccountId, DisplayName = "Renter" },
                new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                startDate,
                startDate.AddDays(2),
                RequestStatus.Open);
        }

        private static Notification BuildNotification(string title, int? relatedRequestId = null)
        {
            return new Notification
            {
                Recipient = new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                Timestamp = new DateTime(2035, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                Title = title,
                Body = $"{title} body",
                Type = NotificationType.Informational,
                RelatedRequest = relatedRequestId.HasValue ? new Request { Id = relatedRequestId.Value } : null,
            };
        }
    }
}
