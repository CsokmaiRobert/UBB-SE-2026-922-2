using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientUserService = BoardRentAndProperty.ApiClient.UserService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class UserServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task GetUsersExceptAsync_WhenServerReturnsList_ReturnsUserListAndUsesExpectedUrl()
        {
            var excludedAccountId = Guid.NewGuid();
            string usersJson = "[ { \"id\":\"" + Guid.NewGuid() + "\", \"displayName\":\"Alice\" } ]";
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, usersJson);
            var userService = BuildService(stubHandler);

            var serviceResult = await userService.GetUsersExceptAsync(excludedAccountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data![0].DisplayName, Is.EqualTo("Alice"));
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/users/except/" + excludedAccountId));
        }

        [Test]
        public async Task GetUsersExceptAsync_WhenServerReturnsError_ReturnsFailure()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.InternalServerError, "{ \"error\":\"db\" }");
            var userService = BuildService(stubHandler);

            var serviceResult = await userService.GetUsersExceptAsync(Guid.NewGuid());

            Assert.That(serviceResult.Success, Is.False);
            Assert.That(serviceResult.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        private static ApiClientUserService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientUserService(stubFactory);
        }
    }
}
