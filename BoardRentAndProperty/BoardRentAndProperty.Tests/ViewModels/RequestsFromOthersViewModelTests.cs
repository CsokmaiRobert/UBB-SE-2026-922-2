using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class RequestsFromOthersViewModelTests
    {
        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private Mock<IRequestService> requestServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;
        private RequestsFromOthersViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestServiceMock = new Mock<IRequestService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();
            this.currentUserContextMock
                .SetupGet(context => context.CurrentUserId)
                .Returns(this.sampleOwnerIdentifier);
            this.requestServiceMock
                .Setup(service => service.GetOpenRequestsForOwner(this.sampleOwnerIdentifier))
                .Returns(ImmutableList<RequestDTO>.Empty);

            this.viewModel = new RequestsFromOthersViewModel(
                this.requestServiceMock.Object,
                this.currentUserContextMock.Object);
        }

        [Test]
        public void TryApproveRequest_WhenServiceSucceeds_ReturnsNull()
        {
            this.requestServiceMock
                .Setup(service => service.ApproveRequest(42, this.sampleOwnerIdentifier))
                .Returns(Result<int, ApproveRequestError>.Success(500));
            this.requestServiceMock.Invocations.Clear();

            string? errorMessage = this.viewModel.TryApproveRequest(42);

            errorMessage.Should().BeNull();
        }

        [Test]
        public void TryDenyRequest_WhenServiceReturnsUnauthorized_ReturnsNonNullErrorMessage()
        {
            this.requestServiceMock
                .Setup(service => service.DenyRequest(
                    42,
                    this.sampleOwnerIdentifier,
                    It.IsAny<string>()))
                .Returns(Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized));

            string? errorMessage = this.viewModel.TryDenyRequest(42, "unavailable");

            errorMessage.Should().NotBeNull();
        }
    }
}
