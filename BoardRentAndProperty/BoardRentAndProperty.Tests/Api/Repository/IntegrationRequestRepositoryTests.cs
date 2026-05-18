using System;
using System.Collections.Immutable;
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
    public sealed class IntegrationRequestRepositoryTests : DataBaseTests
    {
        private RequestRepository requestRepository = null!;
        private NotificationRepository notificationRepository = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestRepository = new RequestRepository(this.DbContextFactory);
            this.notificationRepository = new NotificationRepository(this.DbContextFactory);
        }

        [Test]
        public void AddRequest_ThenGetById_PreservesAllRequestFields()
        {
            int gameId = this.SeedGame(OwnerAccountId, "First Game");
            var newRequest = new Request(
                0,
                new Game { Id = gameId },
                new Account { Id = RenterAccountId, DisplayName = "Renter" },
                new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(4));

            this.requestRepository.Add(newRequest);

            var fetchedRequest = this.requestRepository.Get(newRequest.Id);

            Assert.That(fetchedRequest.Id, Is.EqualTo(newRequest.Id));
            Assert.That(fetchedRequest.Game!.Id, Is.EqualTo(gameId));
            Assert.That(fetchedRequest.Renter!.Id, Is.EqualTo(RenterAccountId));
            Assert.That(fetchedRequest.Owner!.Id, Is.EqualTo(OwnerAccountId));
        }

        [Test]
        public void GetRequestsByGame_WithMultipleGames_ReturnsOnlyMatchingGameRequests()
        {
            int firstGameId = this.SeedGame(OwnerAccountId, "First Game");
            int secondGameId = this.SeedGame(OwnerAccountId, "Second Game");

            var requestForFirstGame = BuildRequest(firstGameId, 50, RequestStatus.Open);
            var requestForSecondGame = BuildRequest(secondGameId, 60, RequestStatus.Open);

            this.requestRepository.Add(requestForFirstGame);
            this.requestRepository.Add(requestForSecondGame);

            var requestsForFirstGame = this.requestRepository.GetRequestsByGame(firstGameId);

            Assert.That(requestsForFirstGame, Is.All.Matches<Request>(request => request.Game!.Id == firstGameId));
            Assert.That(requestsForFirstGame, Has.Some.Matches<Request>(request => request.Id == requestForFirstGame.Id));
            Assert.That(requestsForFirstGame, Has.None.Matches<Request>(request => request.Id == requestForSecondGame.Id));
        }

        [Test]
        public void ApproveAtomically_RemovesOnlyLinkedNotificationsAndCreatesRental()
        {
            int gameId = this.SeedGame(OwnerAccountId, "Approval Game");
            var approvedRequest = BuildRequest(gameId, 50, RequestStatus.Open);
            var conflictingRequest = BuildRequest(gameId, 51, RequestStatus.Open);
            var unrelatedRequest = BuildRequest(gameId, 70, RequestStatus.Open);

            this.requestRepository.Add(approvedRequest);
            this.requestRepository.Add(conflictingRequest);
            this.requestRepository.Add(unrelatedRequest);

            var approvedNotification = BuildNotification("Approved Notification", approvedRequest.Id);
            var conflictingNotification = BuildNotification("Conflicting Notification", conflictingRequest.Id);
            var unrelatedNotification = BuildNotification("Unrelated Notification", unrelatedRequest.Id);

            this.notificationRepository.Add(approvedNotification);
            this.notificationRepository.Add(conflictingNotification);
            this.notificationRepository.Add(unrelatedNotification);

            int rentalId = this.requestRepository.ApproveAtomically(
                approvedRequest,
                ImmutableList.Create(conflictingRequest));

            using var dbContext = this.DbContextFactory.CreateDbContext();
            var remainingNotificationIds = dbContext.Notifications
                .Select(notification => notification.Id)
                .ToList();
            var createdRental = dbContext.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Renter)
                .Include(rental => rental.Owner)
                .Single(rental => rental.Id == rentalId);

            Assert.That(remainingNotificationIds, Has.No.Member(approvedNotification.Id));
            Assert.That(remainingNotificationIds, Has.No.Member(conflictingNotification.Id));
            Assert.That(remainingNotificationIds, Has.Member(unrelatedNotification.Id));
            Assert.That(dbContext.Requests.Any(request => request.Id == approvedRequest.Id), Is.False);
            Assert.That(dbContext.Requests.Any(request => request.Id == conflictingRequest.Id), Is.False);
            Assert.That(dbContext.Requests.Any(request => request.Id == unrelatedRequest.Id), Is.True);
            Assert.That(dbContext.Rentals.Count(), Is.EqualTo(1));
            Assert.That(createdRental.Game!.Id, Is.EqualTo(gameId));
            Assert.That(createdRental.Renter!.Id, Is.EqualTo(RenterAccountId));
            Assert.That(createdRental.Owner!.Id, Is.EqualTo(OwnerAccountId));
            Assert.That(createdRental.StartDate, Is.EqualTo(approvedRequest.StartDate));
            Assert.That(createdRental.EndDate, Is.EqualTo(approvedRequest.EndDate));
        }

        private static Request BuildRequest(
            int gameId,
            int startOffsetInDays,
            RequestStatus status,
            Guid? offeringUserId = null)
        {
            DateTime startDate = new DateTime(2035, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(startOffsetInDays);
            DateTime endDate = startDate.AddDays(2);

            return new Request(
                0,
                new Game { Id = gameId },
                new Account { Id = RenterAccountId, DisplayName = "Renter" },
                new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                startDate,
                endDate,
                status,
                offeringUserId.HasValue ? new Account { Id = offeringUserId.Value, DisplayName = "Offering User" } : null);
        }

        private static Notification BuildNotification(string title, int? relatedRequestId = null)
        {
            return new Notification
            {
                Recipient = new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                Timestamp = new DateTime(2035, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                Title = title,
                Body = $"{title} body",
                Type = NotificationType.Informational,
                RelatedRequest = relatedRequestId.HasValue ? new Request { Id = relatedRequestId.Value } : null,
            };
        }
    }
}
