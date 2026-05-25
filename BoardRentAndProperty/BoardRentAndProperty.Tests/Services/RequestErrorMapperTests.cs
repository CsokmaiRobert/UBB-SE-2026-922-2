using System.Net;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Services;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Services
{
    [TestFixture]
    public sealed class RequestErrorMapperTests
    {
        [Test]
        public void MapCreate_RecognizedErrorCodes_MapToCorrespondingEnums()
        {
            Assert.That(RequestErrorMapper.MapCreate(BuildFailure(HttpStatusCode.BadRequest, "owner_cannot_rent")),
                Is.EqualTo(CreateRequestError.OwnerCannotRent));
            Assert.That(RequestErrorMapper.MapCreate(BuildFailure(HttpStatusCode.Conflict, "dates_unavailable")),
                Is.EqualTo(CreateRequestError.DatesUnavailable));
            Assert.That(RequestErrorMapper.MapCreate(BuildFailure(HttpStatusCode.NotFound, "game_not_found")),
                Is.EqualTo(CreateRequestError.GameDoesNotExist));
            Assert.That(RequestErrorMapper.MapCreate(BuildFailure(HttpStatusCode.BadRequest, "invalid_date_range")),
                Is.EqualTo(CreateRequestError.InvalidDateRange));
        }

        [Test]
        public void MapCreate_WithUnknownCode_FallsBackToInvalidDateRange()
        {
            var fallbackResult = RequestErrorMapper.MapCreate(BuildFailure(HttpStatusCode.BadRequest, "totally_unknown"));

            Assert.That(fallbackResult, Is.EqualTo(CreateRequestError.InvalidDateRange));
        }

        [Test]
        public void MapApprove_ByStatusCode_MapsToCorrespondingEnums()
        {
            Assert.That(RequestErrorMapper.MapApprove(BuildFailure(HttpStatusCode.NotFound)), Is.EqualTo(ApproveRequestError.NotFound));
            Assert.That(RequestErrorMapper.MapApprove(BuildFailure(HttpStatusCode.Forbidden)), Is.EqualTo(ApproveRequestError.Unauthorized));
            Assert.That(RequestErrorMapper.MapApprove(BuildFailure(HttpStatusCode.Conflict, "request_transaction_failed")),
                Is.EqualTo(ApproveRequestError.TransactionFailed));
        }

        [Test]
        public void MapDeny_ByStatusCode_MapsToCorrespondingEnumsAndFallsBackToNotFound()
        {
            Assert.That(RequestErrorMapper.MapDeny(BuildFailure(HttpStatusCode.Forbidden)), Is.EqualTo(DenyRequestError.Unauthorized));
            Assert.That(RequestErrorMapper.MapDeny(BuildFailure(HttpStatusCode.NotFound)), Is.EqualTo(DenyRequestError.NotFound));
            Assert.That(RequestErrorMapper.MapDeny(BuildFailure(HttpStatusCode.InternalServerError)), Is.EqualTo(DenyRequestError.NotFound));
        }

        [Test]
        public void MapCancel_ByStatusCode_MapsToCorrespondingEnumsAndFallsBackToNotFound()
        {
            Assert.That(RequestErrorMapper.MapCancel(BuildFailure(HttpStatusCode.Forbidden)), Is.EqualTo(CancelRequestError.Unauthorized));
            Assert.That(RequestErrorMapper.MapCancel(BuildFailure(HttpStatusCode.NotFound)), Is.EqualTo(CancelRequestError.NotFound));
            Assert.That(RequestErrorMapper.MapCancel(BuildFailure(HttpStatusCode.InternalServerError)), Is.EqualTo(CancelRequestError.NotFound));
        }

        [Test]
        public void MapOffer_RecognizedCodes_MapToCorrespondingEnums()
        {
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.NotFound, "request_not_found")),
                Is.EqualTo(OfferError.NotFound));
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.Forbidden, "request_forbidden")),
                Is.EqualTo(OfferError.NotOwner));
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.Conflict, "request_not_open")),
                Is.EqualTo(OfferError.RequestNotOpen));
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.Conflict, "request_transaction_failed")),
                Is.EqualTo(OfferError.TransactionFailed));
        }

        [Test]
        public void MapOffer_WithoutErrorCode_UsesStatusCodeFallbacks()
        {
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.NotFound)), Is.EqualTo(OfferError.NotFound));
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.Forbidden)), Is.EqualTo(OfferError.NotOwner));
            Assert.That(RequestErrorMapper.MapOffer(BuildFailure(HttpStatusCode.InternalServerError)), Is.EqualTo(OfferError.TransactionFailed));
        }

        private static ServiceResult BuildFailure(HttpStatusCode statusCode, string? errorCode = null) =>
            ServiceResult.Fail("error", statusCode, errorCode);
    }
}
