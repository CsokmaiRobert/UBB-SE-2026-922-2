using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using ApiClientNotificationService = BoardRentAndProperty.ApiClient.NotificationService;

namespace BoardRentAndProperty.Tests.ApiClient
{
    [TestFixture]
    public sealed class NotificationServiceClientTests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://api.test.local/");

        [Test]
        public async Task GetNotificationByIdentifierAsync_ReturnsNotification()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"id\":10, \"title\":\"Welcome\" }");

            var serviceResult = await BuildService(stubHandler).GetNotificationByIdentifierAsync(10);

            Assert.That(serviceResult.Data!.Title, Is.EqualTo("Welcome"));
        }

        [Test]
        public async Task DeleteNotificationByIdentifierAsync_HandlesParsedAndEmptyResponses()
        {
            var parsedHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{ \"id\":5 }");
            var parsedResult = await BuildService(parsedHandler).DeleteNotificationByIdentifierAsync(5);
            Assert.That(parsedResult.Data!.Id, Is.EqualTo(5));
            Assert.That(parsedHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Delete));

            var nullHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "null");
            var fallbackResult = await BuildService(nullHandler).DeleteNotificationByIdentifierAsync(77);
            Assert.That(fallbackResult.Data!.Id, Is.EqualTo(77));
        }

        [Test]
        public async Task UpdateNotificationByIdentifierAsync_UsesPutOnNotificationIdEndpoint()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);

            await BuildService(stubHandler).UpdateNotificationByIdentifierAsync(3, new NotificationDTO { Id = 3 });

            Assert.That(stubHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Put));
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/notifications/3"));
        }

        [Test]
        public async Task GetNotificationsForUserAsync_UsesUserSubpathAndReturnsList()
        {
            var stubHandler = StubHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "[ { \"id\":1 } ]");
            var accountId = Guid.NewGuid();

            var serviceResult = await BuildService(stubHandler).GetNotificationsForUserAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/notifications/user/" + accountId));
        }

        [Test]
        public async Task DeleteLinkedAndPersistNotificationAsync_UseExpectedEndpointsAndVerbs()
        {
            var stubHandler = StubHttpMessageHandler.ReturningStatus(HttpStatusCode.NoContent);
            var notificationService = BuildService(stubHandler);

            await notificationService.DeleteNotificationsLinkedToRequestAsync(42);
            Assert.That(stubHandler.ReceivedRequests[0].Method, Is.EqualTo(HttpMethod.Delete));
            Assert.That(stubHandler.ReceivedRequests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/notifications/request/42"));

            await notificationService.PersistNotificationAsync(new NotificationDTO { Title = "Hello" });
            Assert.That(stubHandler.ReceivedRequests[1].Method, Is.EqualTo(HttpMethod.Put));
            Assert.That(stubHandler.ReceivedRequests[1].RequestUri!.AbsolutePath, Is.EqualTo("/api/notifications/0"));
        }

        private static ApiClientNotificationService BuildService(HttpMessageHandler messageHandler)
        {
            var stubFactory = new StubHttpClientFactory(messageHandler, ApiBaseAddress);
            return new ApiClientNotificationService(stubFactory);
        }
    }
}
