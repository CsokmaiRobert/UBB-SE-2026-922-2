using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientRentalService = BoardRentAndProperty.ApiClient.RentalService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class RentalServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task GetRentalsForRenterAsync_ReturnsRentalsUsingRenterSubpath()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "[ { \"id\":1 } ]");
            var rentalService = BuildService(stubHandler);
            var renterAccountId = Guid.NewGuid();

            var serviceResult = await rentalService.GetRentalsForRenterAsync(renterAccountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/rentals/renter/" + renterAccountId));
        }

        [Test]
        public async Task GetRentalsForOwnerAsync_ReturnsRentalsUsingOwnerSubpath()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "[ { \"id\":7 } ]");
            var rentalService = BuildService(stubHandler);
            var ownerAccountId = Guid.NewGuid();

            var serviceResult = await rentalService.GetRentalsForOwnerAsync(ownerAccountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/rentals/owner/" + ownerAccountId));
        }

        [Test]
        public async Task IsSlotAvailableAsync_PassesDateRangeAsQueryParameters()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "true");
            var rentalService = BuildService(stubHandler);

            var serviceResult = await rentalService.IsSlotAvailableAsync(
                42,
                new DateTime(2030, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2030, 5, 3, 0, 0, 0, DateTimeKind.Utc));

            Assert.That(serviceResult.Data, Is.True);
            string requestQuery = stubHandler.ReceivedRequests[0].RequestUri!.Query;
            Assert.That(requestQuery, Does.Contain("startDate="));
            Assert.That(requestQuery, Does.Contain("endDate="));
        }

        [Test]
        public async Task CreateConfirmedRentalAsync_WhenServerReturnsOk_PostsBodyAndReturnsSuccess()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.OK);
            var rentalService = BuildService(stubHandler);

            var serviceResult = await rentalService.CreateConfirmedRentalAsync(new CreateRentalDataTransferObject
            {
                GameId = 11,
                StartDate = new DateTime(2030, 1, 1),
                EndDate = new DateTime(2030, 1, 3),
            });

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Post));
        }

        [Test]
        public async Task CreateConfirmedRentalAsync_WhenServerReturnsConflict_ReturnsFailure()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(
                HttpStatusCode.Conflict,
                "{ \"error\":\"dates overlap\", \"code\":\"rental_conflict\" }");
            var rentalService = BuildService(stubHandler);

            var serviceResult = await rentalService.CreateConfirmedRentalAsync(new CreateRentalDataTransferObject());

            Assert.That(serviceResult.Success, Is.False);
            Assert.That(serviceResult.ErrorCode, Is.EqualTo("rental_conflict"));
        }

        private static ApiClientRentalService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientRentalService(stubFactory);
        }
    }
}
