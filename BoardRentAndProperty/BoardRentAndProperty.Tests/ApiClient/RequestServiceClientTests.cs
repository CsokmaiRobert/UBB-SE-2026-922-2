using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientRequestService = BoardRentAndProperty.ApiClient.RequestService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class RequestServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task RequestListEndpoints_UseCorrectSubpaths()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "[]");
            var requestService = BuildService(stubHandler);
            var ownerAccountId = Guid.NewGuid();
            var renterAccountId = Guid.NewGuid();

            await requestService.GetRequestsForRenterAsync(renterAccountId);
            await requestService.GetRequestsForOwnerAsync(ownerAccountId);
            await requestService.GetOpenRequestsForOwnerAsync(ownerAccountId);

            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/requests/renter/" + renterAccountId));
            Assert.That(stubHandler.ReceivedRequests[1].RequestUri!.AbsolutePath, Is.EqualTo("/api/requests/owner/" + ownerAccountId));
            Assert.That(stubHandler.ReceivedRequests[2].RequestUri!.AbsolutePath, Is.EqualTo("/api/requests/owner/" + ownerAccountId + "/open"));
        }

        [Test]
        public async Task CreateRequestAsync_HandlesSuccessAndFailureWithCode()
        {
            var successHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"id\":555 }");
            var successResult = await BuildService(successHandler).CreateRequestAsync(new CreateRequestDataTransferObject());
            Assert.That(successResult.Data, Is.EqualTo(555));

            var badRequestHandler = StubHttpMessageHandler.ReturningJson(
                HttpStatusCode.BadRequest,
                "{ \"error\":\"Bad dates.\", \"code\":\"invalid_date_range\" }");
            var failureResult = await BuildService(badRequestHandler).CreateRequestAsync(new CreateRequestDataTransferObject());
            Assert.That(failureResult.ErrorCode, Is.EqualTo("invalid_date_range"));
        }

        [Test]
        public async Task ApproveAndOfferAsync_ReturnRentalIdFromEnvelopedResponse()
        {
            var approveHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"rentalId\":99 }");
            var approveResult = await BuildService(approveHandler).ApproveRequestAsync(1, Guid.NewGuid());
            Assert.That(approveResult.Data, Is.EqualTo(99));
            Assert.That(approveHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/requests/1/approve"));

            var offerHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"rentalId\":201 }");
            var offerResult = await BuildService(offerHandler).OfferGameAsync(8, new RequestActionDataTransferObject());
            Assert.That(offerResult.Data, Is.EqualTo(201));
        }

        [Test]
        public async Task DenyAndCancelAsync_OnSuccess_EchoRequestId()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);
            var requestService = BuildService(stubHandler);

            var denyResult = await requestService.DenyRequestAsync(42, new RequestActionDataTransferObject { Reason = "no" });
            Assert.That(denyResult.Data, Is.EqualTo(42));

            var cancelResult = await requestService.CancelRequestAsync(15, new RequestActionDataTransferObject());
            Assert.That(cancelResult.Data, Is.EqualTo(15));
        }

        [Test]
        public async Task CheckAvailabilityAsync_WhenServerReturnsTrue_ReturnsTrue()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "true");

            var serviceResult = await BuildService(stubHandler).CheckAvailabilityAsync(
                7,
                new DateTime(2030, 6, 1),
                new DateTime(2030, 6, 3));

            Assert.That(serviceResult.Data, Is.True);
        }

        [Test]
        public async Task GetBookedDatesAsync_PassesMonthAndYearAsQueryParameters()
        {
            string rangesJson = "[ { \"startDate\":\"2030-01-05T00:00:00\", \"endDate\":\"2030-01-07T00:00:00\" } ]";
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, rangesJson);

            var serviceResult = await BuildService(stubHandler).GetBookedDatesAsync(5, 1, 2030);

            Assert.That(serviceResult.Data, Has.Count.EqualTo(1));
            string requestQuery = stubHandler.ReceivedRequests[0].RequestUri!.Query;
            Assert.That(requestQuery, Does.Contain("month=1"));
            Assert.That(requestQuery, Does.Contain("year=2030"));
        }

        private static ApiClientRequestService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientRequestService(stubFactory);
        }
    }
}
