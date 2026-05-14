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
    public sealed class RequestsFromOthersViewModelTests
    {
        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private FakeClientRequestService requestService = null!;
        private FakeCurrentUserContext currentUserContext = null!;
        private RequestsFromOthersViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestService = new FakeClientRequestService
            {
                OpenRequestsForOwner = ImmutableList<RequestDTO>.Empty,
            };
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.sampleOwnerIdentifier };

            this.viewModel = new RequestsFromOthersViewModel(
                this.requestService,
                this.currentUserContext);
        }

        [Test]
        public void TryApproveRequest_WhenServiceSucceeds_ReturnsNull()
        {
            this.requestService.ApproveRequestResult = Result<int, ApproveRequestError>.Success(500);

            string? errorMessage = this.viewModel.TryApproveRequest(42);

            Assert.That(errorMessage, Is.Null);
        }

        [Test]
        public void TryDenyRequest_WhenServiceReturnsUnauthorized_ReturnsNonNullErrorMessage()
        {
            this.requestService.DenyRequestResult =
                Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized);

            string? errorMessage = this.viewModel.TryDenyRequest(42, "unavailable");

            Assert.That(errorMessage, Is.Not.Null);
        }
    }
}
