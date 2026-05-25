using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientAdminService = BoardRentAndProperty.ApiClient.AdminService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class AdminServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task GetAllAccountsAsync_WhenServerReturnsList_ReturnsAccountsWithRebasedAvatars()
        {
            string accountsJson = "[ { \"username\":\"alice\", \"avatarUrl\":\"/avatars/alice.png\" } ]";
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, accountsJson);

            var serviceResult = await BuildService(stubHandler).GetAllAccountsAsync(page: 2, pageSize: 50);

            Assert.That(serviceResult.Data![0].AvatarUrl, Is.EqualTo("http://api.test.local/avatars/alice.png"));
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.Query, Does.Contain("page=2"));
        }

        [Test]
        public async Task GetAllAccountsAsync_WhenServerReturnsError_ReturnsFailureWithStatus()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.Forbidden, "{ \"error\":\"Forbidden.\" }");

            var serviceResult = await BuildService(stubHandler).GetAllAccountsAsync(1, 10);

            Assert.That(serviceResult.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task SuspendUnsuspendUnlock_UseCorrespondingEndpoints()
        {
            var accountId = Guid.NewGuid();
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);
            var adminService = BuildService(stubHandler);

            await adminService.SuspendAccountAsync(accountId);
            await adminService.UnsuspendAccountAsync(accountId);
            await adminService.UnlockAccountAsync(accountId);

            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/admin/accounts/" + accountId + "/suspend"));
            Assert.That(stubHandler.ReceivedRequests[1].RequestUri!.AbsolutePath, Is.EqualTo("/api/admin/accounts/" + accountId + "/unsuspend"));
            Assert.That(stubHandler.ReceivedRequests[2].RequestUri!.AbsolutePath, Is.EqualTo("/api/admin/accounts/" + accountId + "/unlock"));
        }

        [Test]
        public async Task ResetPasswordAsync_SendsNewPasswordBody()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);

            await BuildService(stubHandler).ResetPasswordAsync(Guid.NewGuid(), "ChangeMe1!");

            string requestBody = await stubHandler.ReceivedRequests[0].Content!.ReadAsStringAsync();
            Assert.That(requestBody, Does.Contain("ChangeMe1!"));
        }

        private static ApiClientAdminService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientAdminService(stubFactory);
        }
    }
}
