using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using ApiCreateRequestError = BoardRentAndProperty.Api.Services.CreateRequestError;
using ApiApproveRequestError = BoardRentAndProperty.Api.Services.ApproveRequestError;
using ApiDenyRequestError = BoardRentAndProperty.Api.Services.DenyRequestError;
using ApiCancelRequestError = BoardRentAndProperty.Api.Services.CancelRequestError;
using ApiOfferError = BoardRentAndProperty.Api.Services.OfferError;
using ApiBookedDateRange = BoardRentAndProperty.Api.Services.BookedDateRange;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class RequestsControllerTests
    {
        private ConfigurableFakeRequestService requestService = null!;
        private RequestsController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestService = new ConfigurableFakeRequestService();
            this.controller = new RequestsController(this.requestService);
        }

        [Test]
        public void GetForOwner_GetForRenter_GetOpenForOwner_AllReturnOk()
        {
            this.requestService.OwnerRequests = ImmutableList.Create(new RequestDTO { Id = 1 });
            this.requestService.RenterRequests = ImmutableList.Create(new RequestDTO { Id = 2 });
            this.requestService.OpenOwnerRequests = ImmutableList.Create(new RequestDTO { Id = 3 });

            Assert.That(this.controller.GetForOwner(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(this.controller.GetForRenter(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(this.controller.GetOpenForOwner(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public void Create_HandlesSuccessAndAllErrorPaths()
        {
            this.requestService.CreateOutcome = Result<int, ApiCreateRequestError>.Success(555);
            Assert.That(this.controller.Create(new CreateRequestDataTransferObject()).Result, Is.InstanceOf<OkObjectResult>());

            this.requestService.CreateOutcome = Result<int, ApiCreateRequestError>.Failure(ApiCreateRequestError.InvalidDateRange);
            Assert.That(((ObjectResult)this.controller.Create(new CreateRequestDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.BadRequest));

            this.requestService.CreateOutcome = Result<int, ApiCreateRequestError>.Failure(ApiCreateRequestError.GameDoesNotExist);
            Assert.That(((ObjectResult)this.controller.Create(new CreateRequestDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.NotFound));

            this.requestService.CreateOutcome = Result<int, ApiCreateRequestError>.Failure(ApiCreateRequestError.DatesUnavailable);
            Assert.That(((ObjectResult)this.controller.Create(new CreateRequestDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Conflict));
        }

        [Test]
        public void Approve_ReturnsCorrectStatusForEachOutcome()
        {
            this.requestService.ApproveOutcome = Result<int, ApiApproveRequestError>.Success(77);
            Assert.That(this.controller.Approve(1, new RequestActionDataTransferObject()).Result, Is.InstanceOf<OkObjectResult>());

            this.requestService.ApproveOutcome = Result<int, ApiApproveRequestError>.Failure(ApiApproveRequestError.NotFound);
            Assert.That(((ObjectResult)this.controller.Approve(1, new RequestActionDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.NotFound));

            this.requestService.ApproveOutcome = Result<int, ApiApproveRequestError>.Failure(ApiApproveRequestError.Unauthorized);
            Assert.That(((ObjectResult)this.controller.Approve(1, new RequestActionDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Forbidden));

            this.requestService.ApproveOutcome = Result<int, ApiApproveRequestError>.Failure(ApiApproveRequestError.TransactionFailed);
            Assert.That(((ObjectResult)this.controller.Approve(1, new RequestActionDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Conflict));
        }

        [Test]
        public void Deny_ReturnsCorrectStatusForEachOutcome()
        {
            this.requestService.DenyOutcome = Result<int, ApiDenyRequestError>.Success(0);
            Assert.That(this.controller.Deny(1, new RequestActionDataTransferObject()), Is.InstanceOf<NoContentResult>());

            this.requestService.DenyOutcome = Result<int, ApiDenyRequestError>.Failure(ApiDenyRequestError.NotFound);
            Assert.That(((ObjectResult)this.controller.Deny(1, new RequestActionDataTransferObject())).StatusCode,
                Is.EqualTo((int)HttpStatusCode.NotFound));

            this.requestService.DenyOutcome = Result<int, ApiDenyRequestError>.Failure(ApiDenyRequestError.Unauthorized);
            Assert.That(((ObjectResult)this.controller.Deny(1, new RequestActionDataTransferObject())).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Test]
        public void Cancel_ReturnsCorrectStatusForEachOutcome()
        {
            this.requestService.CancelOutcome = Result<int, ApiCancelRequestError>.Success(0);
            Assert.That(this.controller.Cancel(1, new RequestActionDataTransferObject()), Is.InstanceOf<NoContentResult>());

            this.requestService.CancelOutcome = Result<int, ApiCancelRequestError>.Failure(ApiCancelRequestError.NotFound);
            Assert.That(((ObjectResult)this.controller.Cancel(1, new RequestActionDataTransferObject())).StatusCode,
                Is.EqualTo((int)HttpStatusCode.NotFound));

            this.requestService.CancelOutcome = Result<int, ApiCancelRequestError>.Failure(ApiCancelRequestError.Unauthorized);
            Assert.That(((ObjectResult)this.controller.Cancel(1, new RequestActionDataTransferObject())).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Test]
        public void Offer_ReturnsCorrectStatusForEachOutcome()
        {
            this.requestService.OfferOutcome = Result<int, ApiOfferError>.Success(99);
            Assert.That(this.controller.Offer(1, new RequestActionDataTransferObject()).Result, Is.InstanceOf<OkObjectResult>());

            this.requestService.OfferOutcome = Result<int, ApiOfferError>.Failure(ApiOfferError.NotFound);
            Assert.That(((ObjectResult)this.controller.Offer(1, new RequestActionDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.NotFound));

            this.requestService.OfferOutcome = Result<int, ApiOfferError>.Failure(ApiOfferError.NotOwner);
            Assert.That(((ObjectResult)this.controller.Offer(1, new RequestActionDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Forbidden));

            this.requestService.OfferOutcome = Result<int, ApiOfferError>.Failure(ApiOfferError.RequestNotOpen);
            Assert.That(((ObjectResult)this.controller.Offer(1, new RequestActionDataTransferObject()).Result!).StatusCode,
                Is.EqualTo((int)HttpStatusCode.Conflict));
        }

        [Test]
        public void GetBookedDates_ReturnsOkWithMappedDateRanges()
        {
            this.requestService.BookedDates = ImmutableList.Create(
                new ApiBookedDateRange { StartDate = new DateTime(2030, 1, 1), EndDate = new DateTime(2030, 1, 3) });

            var controllerResponse = this.controller.GetBookedDates(1);
            var okResult = controllerResponse.Result as OkObjectResult;
            var bookedRanges = okResult!.Value as List<BookedDateRangeDataTransferObject>;

            Assert.That(bookedRanges, Has.Count.EqualTo(1));
        }

        [Test]
        public void CheckAvailability_ReturnsOkWithAvailabilityResult()
        {
            this.requestService.AvailabilityResult = false;

            var controllerResponse = this.controller.CheckAvailability(1, new DateTime(2030, 1, 1), new DateTime(2030, 1, 5));
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.EqualTo(false));
        }
    }
}
