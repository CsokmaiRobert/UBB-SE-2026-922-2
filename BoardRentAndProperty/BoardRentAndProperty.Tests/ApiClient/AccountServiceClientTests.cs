using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientAccountService = BoardRentAndProperty.ApiClient.AccountService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class AccountServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task GetProfileAsync_WhenServerReturnsOkWithJson_ReturnsProfileWithRebasedAvatar()
        {
            var accountId = Guid.NewGuid();
            string profileJson = "{ \"id\":\"" + accountId + "\", \"username\":\"alice\", \"avatarUrl\":\"/avatars/alice.png\" }";
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, profileJson);
            var accountService = BuildService(stubHandler);

            var serviceResult = await accountService.GetProfileAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data!.AvatarUrl, Is.EqualTo("http://api.test.local/avatars/alice.png"));
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/accounts/" + accountId));
        }

        [Test]
        public async Task GetProfileAsync_TransportFailures_ReturnsCorrespondingStatusCodes()
        {
            var connectionRefusedHandler = StubHttpMessageHandler.Throwing(new HttpRequestException("connection refused"));
            var unavailableResult = await BuildService(connectionRefusedHandler).GetProfileAsync(Guid.NewGuid());
            Assert.That(unavailableResult.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));

            var timeoutHandler = StubHttpMessageHandler.Throwing(new TaskCanceledException());
            var timeoutResult = await BuildService(timeoutHandler).GetProfileAsync(Guid.NewGuid());
            Assert.That(timeoutResult.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));
        }

        [Test]
        public async Task GetProfileAsync_WhenServerReturnsErrorEnvelope_ReturnsFailureWithStatusAndCode()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(
                HttpStatusCode.NotFound,
                "{ \"error\":\"Account not found.\", \"code\":\"not_found\" }");

            var serviceResult = await BuildService(stubHandler).GetProfileAsync(Guid.NewGuid());

            Assert.That(serviceResult.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(serviceResult.ErrorCode, Is.EqualTo("not_found"));
        }

        [Test]
        public async Task UpdateProfileAndChangePasswordAndRemoveAvatar_OnSuccess_ReturnSuccessUsingCorrectHttpVerb()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);
            var accountService = BuildService(stubHandler);
            var accountId = Guid.NewGuid();

            var updateResult = await accountService.UpdateProfileAsync(accountId, new AccountProfileDataTransferObject());
            Assert.That(updateResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Put));

            var changePasswordResult = await accountService.ChangePasswordAsync(accountId, "Old1!", "NewWord2!");
            Assert.That(changePasswordResult.Success, Is.True);

            var removeAvatarResult = await accountService.RemoveAvatarAsync(accountId);
            Assert.That(removeAvatarResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[2].Method, Is.EqualTo(HttpMethod.Delete));
        }

        [Test]
        public async Task UploadAvatarAsync_WhenServerAcceptsFile_ReturnsAbsoluteAvatarUrl()
        {
            string temporaryImagePath = Path.Combine(Path.GetTempPath(), "brap-test-avatar-" + Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(temporaryImagePath, new byte[] { 1, 2, 3 });
            try
            {
                var stubHandler = StubHttpMessageHandler.ReturningJson(
                    HttpStatusCode.OK,
                    "{ \"avatarUrl\":\"/avatars/uploaded.png\" }");

                var serviceResult = await BuildService(stubHandler).UploadAvatarAsync(Guid.NewGuid(), temporaryImagePath);

                Assert.That(serviceResult.Data, Is.EqualTo("http://api.test.local/avatars/uploaded.png"));
            }
            finally
            {
                File.Delete(temporaryImagePath);
            }
        }

        [Test]
        public async Task UploadAvatarAsync_WhenLocalFileMissing_ReturnsFileFailure()
        {
            string missingPath = Path.Combine(Path.GetTempPath(), "definitely-missing-file-" + Guid.NewGuid().ToString("N") + ".png");
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{}");

            var serviceResult = await BuildService(stubHandler).UploadAvatarAsync(Guid.NewGuid(), missingPath);

            Assert.That(serviceResult.Success, Is.False);
        }

        private static ApiClientAccountService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientAccountService(stubFactory);
        }
    }
}
