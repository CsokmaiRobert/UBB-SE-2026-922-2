using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientGameService = BoardRentAndProperty.ApiClient.GameService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class GameServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task CreateGameAsync_WhenServerReturnsOk_PostsBodyAndReturnsSuccess()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.OK);

            var serviceResult = await BuildService(stubHandler).CreateGameAsync(new GameDTO { Name = "Catan" });

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Post));
            string requestBody = await stubHandler.ReceivedRequests[0].Content!.ReadAsStringAsync();
            Assert.That(requestBody, Does.Contain("Catan"));
        }

        [Test]
        public async Task CreateGameAsync_WhenServerReturnsValidationFailure_ReturnsFailureWithErrorCode()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(
                HttpStatusCode.BadRequest,
                "{ \"error\":\"Name too short.\", \"code\":\"game_validation_failed\" }");

            var serviceResult = await BuildService(stubHandler).CreateGameAsync(new GameDTO());

            Assert.That(serviceResult.ErrorCode, Is.EqualTo("game_validation_failed"));
        }

        [Test]
        public async Task DeleteGameAsync_HandlesSuccessAndConflict()
        {
            var successHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"id\":33, \"name\":\"Pandemic\" }");
            var successResult = await BuildService(successHandler).DeleteGameAsync(33);
            Assert.That(successResult.Data!.Name, Is.EqualTo("Pandemic"));

            var conflictHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.Conflict, "{ \"error\":\"Has rentals.\" }");
            var conflictResult = await BuildService(conflictHandler).DeleteGameAsync(5);
            Assert.That(conflictResult.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task GetGameByIdAsync_WhenServerReturnsJson_ReturnsGame()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"id\":11, \"name\":\"Risk\" }");

            var serviceResult = await BuildService(stubHandler).GetGameByIdAsync(11);

            Assert.That(serviceResult.Data!.Id, Is.EqualTo(11));
        }

        [Test]
        public async Task GameListEndpoints_AllReturnListAndUseExpectedSubpaths()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "[ { \"id\":1 } ]");
            var gameService = BuildService(stubHandler);
            var ownerAccountId = Guid.NewGuid();
            var renterAccountId = Guid.NewGuid();

            await gameService.GetAllGamesAsync();
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/games"));

            await gameService.GetGamesForOwnerAsync(ownerAccountId);
            Assert.That(stubHandler.ReceivedRequests[1].RequestUri!.AbsolutePath, Is.EqualTo("/api/games/owner/" + ownerAccountId));

            await gameService.GetActiveGamesForOwnerAsync(ownerAccountId);
            Assert.That(stubHandler.ReceivedRequests[2].RequestUri!.AbsolutePath, Is.EqualTo("/api/games/owner/" + ownerAccountId + "/active"));

            await gameService.GetAvailableGamesForRenterAsync(renterAccountId);
            Assert.That(stubHandler.ReceivedRequests[3].RequestUri!.AbsolutePath, Is.EqualTo("/api/games/renter/" + renterAccountId + "/available"));
        }

        [Test]
        public async Task UpdateGameAsync_UsesPutOnGameIdEndpoint()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);

            await BuildService(stubHandler).UpdateGameAsync(7, new GameDTO { Id = 7 });

            Assert.That(stubHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Put));
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/games/7"));
        }

        private static ApiClientGameService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientGameService(stubFactory);
        }
    }
}
