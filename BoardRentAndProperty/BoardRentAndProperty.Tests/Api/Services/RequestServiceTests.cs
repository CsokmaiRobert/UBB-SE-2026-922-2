using System;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.Models;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;

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
        public void CreateRequest_WhenRenterIsOwner_ReturnsOwnerCannotRent()
        {
            var ownerId = Guid.NewGuid();
            this.gameRepository.GamesById[10] = new Game
                {
                    Id = 10,
                    Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                    IsActive = true,
                };

            var result = this.service.CreateRequest(
                10,
                ownerId,
                ownerId,
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(4));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.OwnerCannotRent));
        }

        [Test]
        public void CreateRequest_WhenDateRangeIsInvalid_ReturnsInvalidDateRange()
        {
            var result = this.service.CreateRequest(
                10,
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(4),
                DateTime.UtcNow.AddDays(2));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.InvalidDateRange));
        }

        [Test]
        public void CancelRequest_AsRenter_DeletesRequestAndNotifications()
        {
            var renterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var existingRequest = new Request
            {
                Id = 100,
                Renter = new Account { Id = renterId, DisplayName = "Renter" },
                Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                Game = new Game { Id = 10, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RequestStatus.Open,
            };

            this.requestRepository.RequestsById[100] = existingRequest;

            var result = this.service.CancelRequest(100, renterId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(100));
            Assert.That(this.requestRepository.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.requestRepository.LastDeletedRequestId, Is.EqualTo(100));
            Assert.That(this.notificationService.DeleteLinkedNotificationCallCount, Is.EqualTo(1));
            Assert.That(this.notificationService.LastLinkedRequestId, Is.EqualTo(100));
        }
    }
}
