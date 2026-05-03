using System;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.Models;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class RequestServiceTests
    {
        private Mock<IRequestRepository> requestRepositoryMock = null!;
        private Mock<IRentalRepository> rentalRepositoryMock = null!;
        private Mock<IGameRepository> gameRepositoryMock = null!;
        private Mock<INotificationService> notificationServiceMock = null!;
        private RequestService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestRepositoryMock = new Mock<IRequestRepository>();
            this.rentalRepositoryMock = new Mock<IRentalRepository>();
            this.gameRepositoryMock = new Mock<IGameRepository>();
            this.notificationServiceMock = new Mock<INotificationService>();

            this.service = new RequestService(
                this.requestRepositoryMock.Object,
                this.rentalRepositoryMock.Object,
                this.gameRepositoryMock.Object,
                this.notificationServiceMock.Object,
                new RequestMapper(new GameMapper(new UserMapper()), new UserMapper()));
        }

        [Test]
        public void CreateRequest_WhenRenterIsOwner_ReturnsOwnerCannotRent()
        {
            var ownerId = Guid.NewGuid();
            this.gameRepositoryMock
                .Setup(repository => repository.Get(10))
                .Returns(new Game
                {
                    Id = 10,
                    Owner = new Account { Id = ownerId, DisplayName = "Owner" },
                    IsActive = true,
                });

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

            this.requestRepositoryMock
                .Setup(repository => repository.Get(100))
                .Returns(existingRequest);

            var result = this.service.CancelRequest(100, renterId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(100));
            this.requestRepositoryMock.Verify(repository => repository.Delete(100), Times.Once);
            this.notificationServiceMock.Verify(service => service.DeleteNotificationsLinkedToRequest(100), Times.Once);
        }
    }
}
