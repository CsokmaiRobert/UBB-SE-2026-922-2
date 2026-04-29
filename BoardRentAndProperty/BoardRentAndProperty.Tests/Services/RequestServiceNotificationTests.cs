using System;
using BoardRentAndProperty;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Services
{
    [TestFixture]
    public class RequestServiceNotificationTests
    {
        private Mock<IRequestRepository> requestRepository = null!;
        private Mock<IRentalRepository> rentalRepository = null!;
        private Mock<IGameRepository> gameRepository = null!;
        private Mock<INotificationService> notificationService = null!;
        private Mock<IMapper<Request, RequestDTO>> requestMapper = null!;
        private RequestService requestService = null!;

        [SetUp]
        public void SetUp()
        {
            requestRepository = new Mock<IRequestRepository>();
            rentalRepository = new Mock<IRentalRepository>();
            gameRepository = new Mock<IGameRepository>();
            notificationService = new Mock<INotificationService>();
            requestMapper = new Mock<IMapper<Request, RequestDTO>>();

            requestService = new RequestService(
                requestRepository.Object,
                rentalRepository.Object,
                gameRepository.Object,
                notificationService.Object,
                requestMapper.Object);
        }

        [Test]
        public void DenyRequestDeletesLinkedNotificationsBeforeDeletingRequest()
        {
            const int requestId = 7;
            var request = BuildRequest(requestId, renterId: 2, ownerId: 5);
            requestRepository.Setup(repository => repository.Get(requestId)).Returns(request);
            requestRepository.Setup(repository => repository.Delete(requestId)).Returns(request);

            var result = requestService.DenyRequest(requestId, denyingOwnerId: 5, denialReason: "Unavailable");

            Assert.That(result.IsSuccess, Is.True);
            notificationService.Verify(service => service.DeleteNotificationsLinkedToRequest(requestId), Times.Once);
            requestRepository.Verify(repository => repository.Delete(requestId), Times.Once);
            notificationService.Verify(
                service => service.SendNotificationToUser(
                    2,
                    It.Is<NotificationDTO>(notification =>
                        notification.Title == Constants.NotificationTitles.RentalRequestDeclined)),
                Times.Once);
        }

        [Test]
        public void CancelRequestDeletesLinkedNotificationsBeforeDeletingRequest()
        {
            const int requestId = 8;
            var request = BuildRequest(requestId, renterId: 2, ownerId: 5);
            requestRepository.Setup(repository => repository.Get(requestId)).Returns(request);
            requestRepository.Setup(repository => repository.Delete(requestId)).Returns(request);

            var result = requestService.CancelRequest(requestId, cancellingRenterUserId: 2);

            Assert.That(result.IsSuccess, Is.True);
            notificationService.Verify(service => service.DeleteNotificationsLinkedToRequest(requestId), Times.Once);
            requestRepository.Verify(repository => repository.Delete(requestId), Times.Once);
        }

        private static Request BuildRequest(int requestId, int renterId, int ownerId)
        {
            return new Request(
                requestId,
                new Game
                {
                    Id = 10,
                    Name = "Catan"
                },
                new User
                {
                    Id = renterId,
                    DisplayName = "Renter"
                },
                new User
                {
                    Id = ownerId,
                    DisplayName = "Owner"
                },
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(4));
        }
    }
}
