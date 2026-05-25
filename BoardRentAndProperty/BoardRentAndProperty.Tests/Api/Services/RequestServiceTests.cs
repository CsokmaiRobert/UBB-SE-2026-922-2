using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.Models;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using RequestService = BoardRentAndProperty.Api.Services.RequestService;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class RequestServiceTests
    {
        private FakeRequestRepository requestRepository = null!;
        private FakeRentalRepository rentalRepository = null!;
        private FakeGameRepository gameRepository = null!;
        private FakeApiNotificationService notificationService = null!;
        private RequestService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestRepository = new FakeRequestRepository();
            this.rentalRepository = new FakeRentalRepository();
            this.gameRepository = new FakeGameRepository();
            this.notificationService = new FakeApiNotificationService();
            this.service = new RequestService(
                this.requestRepository,
                this.rentalRepository,
                this.gameRepository,
                this.notificationService,
                new RequestMapper(new GameMapper(new UserMapper()), new UserMapper()));
        }

        [Test]
        public void CreateRequest_HandlesAllFailurePathsAndSuccessfulCreation()
        {
            var invalidDateResult = this.service.CreateRequest(
                10, Guid.NewGuid(), Guid.NewGuid(),
                DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(2));
            Assert.That(invalidDateResult.Error, Is.EqualTo(CreateRequestError.InvalidDateRange));

            var ownerId = Guid.NewGuid();
            this.gameRepository.GamesById[10] = new Game
            {
                Id = 10,
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                IsActive = true,
            };
            var ownerCannotRentResult = this.service.CreateRequest(
                10, ownerId, ownerId,
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(4));
            Assert.That(ownerCannotRentResult.Error, Is.EqualTo(CreateRequestError.OwnerCannotRent));

            var renterId = Guid.NewGuid();
            var successResult = this.service.CreateRequest(
                10, renterId, ownerId,
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(4));
            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(this.requestRepository.AddCallCount, Is.EqualTo(1));
        }

        [Test]
        public void ApproveRequest_HandlesNotFoundUnauthorizedAndSuccess()
        {
            var ownerId = Guid.NewGuid();

            var notFoundResult = this.service.ApproveRequest(999, ownerId);
            Assert.That(notFoundResult.Error, Is.EqualTo(ApproveRequestError.NotFound));

            this.requestRepository.RequestsById[200] = new Request
            {
                Id = 200,
                Renter = new Account { Id = Guid.NewGuid(), DisplayName = "Renter" },
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                Game = new Game { Id = 5, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(4),
                Status = RequestStatus.Open,
            };
            var unauthorizedResult = this.service.ApproveRequest(200, Guid.NewGuid());
            Assert.That(unauthorizedResult.Error, Is.EqualTo(ApproveRequestError.Unauthorized));

            this.requestRepository.ApproveAtomicallyResult = 1;
            var successResult = this.service.ApproveRequest(200, ownerId);
            Assert.That(successResult.IsSuccess, Is.True);
        }

        [Test]
        public void DenyRequest_HandlesNotFoundUnauthorizedAndSuccessWithNotification()
        {
            var ownerId = Guid.NewGuid();
            var renterId = Guid.NewGuid();

            Assert.That(this.service.DenyRequest(999, ownerId, "no").Error, Is.EqualTo(DenyRequestError.NotFound));

            this.requestRepository.RequestsById[150] = new Request
            {
                Id = 150,
                Renter = new Account { Id = renterId, DisplayName = "Renter" },
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                Game = new Game { Id = 5, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(4),
                Status = RequestStatus.Open,
            };

            Assert.That(this.service.DenyRequest(150, Guid.NewGuid(), "no").Error, Is.EqualTo(DenyRequestError.Unauthorized));

            var successResult = this.service.DenyRequest(150, ownerId, "Not available.");
            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(this.notificationService.DeleteLinkedNotificationCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.SendNotificationCallCount, Is.EqualTo(1));
        }

        [Test]
        public void OfferGame_HandlesNotFoundNotOwnerAndRequestNotOpen()
        {
            var ownerId = Guid.NewGuid();

            Assert.That(this.service.OfferGame(999, ownerId).Error, Is.EqualTo(OfferError.NotFound));

            this.requestRepository.RequestsById[300] = new Request
            {
                Id = 300,
                Renter = new Account { Id = Guid.NewGuid(), DisplayName = "Renter" },
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                Game = new Game { Id = 5, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(4),
                Status = RequestStatus.Accepted,
            };
            Assert.That(this.service.OfferGame(300, Guid.NewGuid()).Error, Is.EqualTo(OfferError.NotOwner));
            Assert.That(this.service.OfferGame(300, ownerId).Error, Is.EqualTo(OfferError.RequestNotOpen));
        }

        [Test]
        public void CheckAvailability_RespectsActiveGameAndFutureWindow()
        {
            this.gameRepository.GamesById[1] = new Game { Id = 1, IsActive = true };
            Assert.That(
                this.service.CheckAvailability(1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3)),
                Is.True);

            Assert.That(
                this.service.CheckAvailability(1, DateTime.UtcNow.AddDays(60), DateTime.UtcNow.AddDays(62)),
                Is.False);

            this.gameRepository.GamesById[2] = new Game { Id = 2, IsActive = false };
            Assert.That(
                this.service.CheckAvailability(2, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3)),
                Is.False);
        }

        [Test]
        public void OnGameDeactivated_CancelsPendingRequestsForGame()
        {
            int gameId = 99;
            var pendingRequest = new Request
            {
                Id = 401,
                Renter = new Account { Id = Guid.NewGuid(), DisplayName = "Renter" },
                Owner = new Account { Id = Guid.NewGuid(), DisplayName = "Owner" },
                Game = new Game { Id = gameId, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(4),
                Status = RequestStatus.Open,
            };
            this.requestRepository.RequestsById[401] = pendingRequest;
            this.requestRepository.RequestsByGame = ImmutableList.Create(pendingRequest);

            this.service.OnGameDeactivated(gameId);

            Assert.That(this.requestRepository.DeleteCallCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(this.notificationService.SendNotificationCallCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void GetBookedDates_FiltersByMonthAndYearAndAppliesBuffer()
        {
            int gameId = 50;
            var requestInMonth = new Request
            {
                Id = 1,
                Game = new Game { Id = gameId },
                StartDate = new DateTime(2030, 3, 5, 12, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2030, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                Status = RequestStatus.Open,
            };
            var requestInOtherMonth = new Request
            {
                Id = 2,
                Game = new Game { Id = gameId },
                StartDate = new DateTime(2030, 4, 5, 12, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2030, 4, 7, 12, 0, 0, DateTimeKind.Utc),
            };
            this.requestRepository.RequestsByGame = ImmutableList.Create(requestInMonth, requestInOtherMonth);

            var bookedRanges = this.service.GetBookedDates(gameId, 3, 2030);

            Assert.That(bookedRanges, Has.Count.EqualTo(1));
            Assert.That(bookedRanges[0].StartDate, Is.EqualTo(requestInMonth.StartDate));
        }

        [Test]
        public void GetRequestsForRenterAndOwner_ReturnMappedDtosFromRepository()
        {
            var renterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var sampleRequest = new Request
            {
                Id = 10,
                Renter = new Account { Id = renterId, DisplayName = "Renter" },
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                Game = new Game { Id = 5, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = RequestStatus.Open,
            };
            this.requestRepository.RequestsByRenter = ImmutableList.Create(sampleRequest);
            this.requestRepository.RequestsByOwner = ImmutableList.Create(sampleRequest);

            Assert.That(this.service.GetRequestsForRenter(renterId).Single().Id, Is.EqualTo(10));
            Assert.That(this.service.GetRequestsForOwner(ownerId).Single().Id, Is.EqualTo(10));
            Assert.That(this.service.GetOpenRequestsForOwner(ownerId).Single().Status, Is.EqualTo(RequestStatus.Open));
        }

        [Test]
        public void CancelRequest_AsRenter_DeletesRequestAndNotifications()
        {
            var renterId = Guid.NewGuid();
            this.requestRepository.RequestsById[100] = new Request
            {
                Id = 100,
                Renter = new Account { Id = renterId, DisplayName = "Renter" },
                Owner = new Account { Id = Guid.NewGuid(), DisplayName = "Owner" },
                Game = new Game { Id = 10, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RequestStatus.Open,
            };

            var result = this.service.CancelRequest(100, renterId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(this.requestRepository.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.DeleteLinkedNotificationCallCount, Is.EqualTo(1));
        }
    }
}
