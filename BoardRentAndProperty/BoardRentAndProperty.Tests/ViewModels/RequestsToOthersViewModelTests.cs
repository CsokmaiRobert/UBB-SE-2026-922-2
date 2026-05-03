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
    public sealed class RequestsToOthersViewModelTests
    {
        [Test]
        public void LoadRequests_WithMultipleRequests_SetsRenterIdAndOrdersByStartDateDescending()
        {
            var requestServiceMock = new Mock<IRequestService>();
            var currentUserContextMock = new Mock<ICurrentUserContext>();
            var currentUserId = Guid.NewGuid();

            currentUserContextMock.Setup(context => context.CurrentUserId).Returns(currentUserId);

            var firstRequest = new RequestDTO { Id = 10, StartDate = new DateTime(2025, 1, 1) };
            var secondRequest = new RequestDTO { Id = 11, StartDate = new DateTime(2025, 1, 5) };

            requestServiceMock
                .Setup(service => service.GetRequestsForRenter(currentUserId))
                .Returns(ImmutableList.Create(firstRequest, secondRequest));

            var viewModel = new RequestsToOthersViewModel(requestServiceMock.Object, currentUserContextMock.Object);

            viewModel.LoadRequests();

            Assert.That(viewModel.CurrentRenterUserId, Is.EqualTo(currentUserId));
            Assert.That(viewModel.PagedItems, Has.Count.EqualTo(2));
            Assert.That(viewModel.PagedItems[0].Id, Is.EqualTo(11));
            Assert.That(viewModel.PagedItems[1].Id, Is.EqualTo(10));
        }

        [Test]
        public void TryCancelRequest_WhenServiceSucceeds_ReturnsNull()
        {
            var requestServiceMock = new Mock<IRequestService>();
            var currentUserContextMock = new Mock<ICurrentUserContext>();
            var currentUserId = Guid.NewGuid();

            currentUserContextMock.Setup(context => context.CurrentUserId).Returns(currentUserId);
            requestServiceMock
                .Setup(service => service.GetRequestsForRenter(currentUserId))
                .Returns(ImmutableList<RequestDTO>.Empty);

            var viewModel = new RequestsToOthersViewModel(requestServiceMock.Object, currentUserContextMock.Object);
            int requestIdToCancel = 100;

            requestServiceMock
                .Setup(service => service.CancelRequest(requestIdToCancel, currentUserId))
                .Returns(Result<int, CancelRequestError>.Success(requestIdToCancel));

            string cancellationErrorMessage = viewModel.TryCancelRequest(requestIdToCancel);

            Assert.That(cancellationErrorMessage, Is.Null);
        }

        [Test]
        public void TryCancelRequest_WhenRequestNotFound_ReturnsNotFoundErrorMessage()
        {
            var requestServiceMock = new Mock<IRequestService>();
            var currentUserContextMock = new Mock<ICurrentUserContext>();
            var currentUserId = Guid.NewGuid();

            currentUserContextMock.Setup(context => context.CurrentUserId).Returns(currentUserId);
            requestServiceMock
                .Setup(service => service.GetRequestsForRenter(currentUserId))
                .Returns(ImmutableList<RequestDTO>.Empty);

            var viewModel = new RequestsToOthersViewModel(requestServiceMock.Object, currentUserContextMock.Object);
            int requestIdToCancel = 100;

            requestServiceMock
                .Setup(service => service.CancelRequest(requestIdToCancel, currentUserId))
                .Returns(Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound));

            string cancellationErrorMessage = viewModel.TryCancelRequest(requestIdToCancel);

            Assert.That(cancellationErrorMessage, Is.EqualTo("Request not found."));
        }
    }
}
