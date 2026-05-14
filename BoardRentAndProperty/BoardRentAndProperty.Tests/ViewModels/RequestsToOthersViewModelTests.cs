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
    public sealed class RequestsToOthersViewModelTests
    {
        [Test]
        public void LoadRequests_WithMultipleRequests_SetsRenterIdAndOrdersByStartDateDescending()
        {
            var currentUserId = Guid.NewGuid();
            var currentUserContext = new FakeCurrentUserContext { CurrentUserId = currentUserId };

            var firstRequest = new RequestDTO { Id = 10, StartDate = new DateTime(2025, 1, 1) };
            var secondRequest = new RequestDTO { Id = 11, StartDate = new DateTime(2025, 1, 5) };

            var requestService = new FakeClientRequestService
            {
                RequestsForRenter = ImmutableList.Create(firstRequest, secondRequest),
            };

            var viewModel = new RequestsToOthersViewModel(requestService, currentUserContext);

            viewModel.LoadRequests();

            Assert.That(viewModel.CurrentRenterUserId, Is.EqualTo(currentUserId));
            Assert.That(viewModel.PagedItems, Has.Count.EqualTo(2));
            Assert.That(viewModel.PagedItems[0].Id, Is.EqualTo(11));
            Assert.That(viewModel.PagedItems[1].Id, Is.EqualTo(10));
        }

        [Test]
        public void TryCancelRequest_WhenServiceSucceeds_ReturnsNull()
        {
            var currentUserId = Guid.NewGuid();
            var requestService = new FakeClientRequestService();
            var currentUserContext = new FakeCurrentUserContext { CurrentUserId = currentUserId };

            var viewModel = new RequestsToOthersViewModel(requestService, currentUserContext);
            int requestIdToCancel = 100;
            requestService.CancelRequestResult = Result<int, CancelRequestError>.Success(requestIdToCancel);

            string? cancellationErrorMessage = viewModel.TryCancelRequest(requestIdToCancel);

            Assert.That(cancellationErrorMessage, Is.Null);
        }

        [Test]
        public void TryCancelRequest_WhenRequestNotFound_ReturnsNotFoundErrorMessage()
        {
            var currentUserId = Guid.NewGuid();
            var requestService = new FakeClientRequestService();
            var currentUserContext = new FakeCurrentUserContext { CurrentUserId = currentUserId };

            var viewModel = new RequestsToOthersViewModel(requestService, currentUserContext);
            int requestIdToCancel = 100;
            requestService.CancelRequestResult =
                Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound);

            string? cancellationErrorMessage = viewModel.TryCancelRequest(requestIdToCancel);

            Assert.That(cancellationErrorMessage, Is.EqualTo("Request not found."));
        }
    }
}
