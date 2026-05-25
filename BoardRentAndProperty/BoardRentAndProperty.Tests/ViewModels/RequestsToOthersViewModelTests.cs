using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
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
        public async Task LoadRequests_WithMultipleRequests_SetsRenterIdAndOrdersByStartDateDescending()
        {
            var currentUserId = Guid.NewGuid();
            var currentUserContext = new FakeCurrentUserContext { CurrentUserId = currentUserId };
            var requestService = new FakeClientRequestService
            {
                RequestsForRenter = ImmutableList.Create(
                    new RequestDTO { Id = 10, StartDate = new DateTime(2025, 1, 1) },
                    new RequestDTO { Id = 11, StartDate = new DateTime(2025, 1, 5) }),
            };
            var viewModel = new RequestsToOthersViewModel(requestService, currentUserContext);

            await viewModel.LoadRequestsAsync();

            Assert.That(viewModel.CurrentRenterUserId, Is.EqualTo(currentUserId));
            Assert.That(viewModel.PagedItems[0].Id, Is.EqualTo(11));
        }

        [Test]
        public async Task TryCancelRequest_ReturnsNullOnSuccessAndFriendlyMessageOnFailure()
        {
            var currentUserContext = new FakeCurrentUserContext { CurrentUserId = Guid.NewGuid() };
            var requestService = new FakeClientRequestService();
            var viewModel = new RequestsToOthersViewModel(requestService, currentUserContext);

            requestService.CancelRequestResult = Result<int, CancelRequestError>.Success(100);
            Assert.That(await viewModel.TryCancelRequestAsync(100), Is.Null);

            requestService.CancelRequestResult = Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound);
            Assert.That(await viewModel.TryCancelRequestAsync(100), Is.EqualTo("Request not found."));
        }
    }
}
