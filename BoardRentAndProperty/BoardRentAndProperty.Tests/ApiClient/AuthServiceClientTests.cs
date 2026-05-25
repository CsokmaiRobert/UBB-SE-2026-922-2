using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientAuthService = BoardRentAndProperty.ApiClient.AuthService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class AuthServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task RegisterAsync_WhenServerReturnsOk_ReturnsSuccessAndPostsBody()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.OK, "{}");

            var serviceResult = await BuildService(stubHandler).RegisterAsync(new RegisterDataTransferObject { Username = "new_user" });

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/auth/register"));
        }

        [Test]
        public async Task LoginAsync_ReturnsRebasedAvatarProfileOnSuccessAndStatusOnFailure()
        {
            var successHandler = StubHttpMessageHandler.ReturningJson(
                HttpStatusCode.OK,
                "{ \"username\":\"alice\", \"avatarUrl\":\"/avatars/alice.png\" }");
            var successResult = await BuildService(successHandler).LoginAsync(new LoginDataTransferObject());
            Assert.That(successResult.Data!.AvatarUrl, Is.EqualTo("http://api.test.local/avatars/alice.png"));

            var unauthorizedHandler = StubHttpMessageHandler.ReturningJson(
                HttpStatusCode.Unauthorized,
                "{ \"error\":\"Invalid username or password.\" }");
            var failureResult = await BuildService(unauthorizedHandler).LoginAsync(new LoginDataTransferObject());
            Assert.That(failureResult.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task LogoutAsync_AndForgotPasswordAsync_ReturnSuccessWhenServerResponds()
        {
            var logoutHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);
            var logoutResult = await BuildService(logoutHandler).LogoutAsync();
            Assert.That(logoutResult.Success, Is.True);
            Assert.That(logoutHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Post));

            var forgotHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.OK, "reset-token-value");
            var forgotResult = await BuildService(forgotHandler).ForgotPasswordAsync();
            Assert.That(forgotResult.Data, Is.EqualTo("reset-token-value"));
        }

        [Test]
        public async Task LogoutAsync_WhenTransportFails_ReturnsServiceUnavailable()
        {
            var stubHandler = StubHttpMessageHandler.Throwing(new HttpRequestException("dropped"));

            var serviceResult = await BuildService(stubHandler).LogoutAsync();

            Assert.That(serviceResult.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        }

        private static ApiClientAuthService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientAuthService(stubFactory);
        }
    }
}
