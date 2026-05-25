using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder;

        public List<HttpRequestMessage> ReceivedRequests { get; } = new List<HttpRequestMessage>();

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            this.responder = responder;
        }

        public static StubHttpMessageHandler ReturningJson(HttpStatusCode statusCode, string jsonBody) =>
            new StubHttpMessageHandler((request, cancellationToken) =>
            {
                var jsonResponse = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"),
                };
                return Task.FromResult(jsonResponse);
            });

        public static StubHttpMessageHandler ReturningStatus(HttpStatusCode statusCode, string body = "") =>
            new StubHttpMessageHandler((request, cancellationToken) =>
            {
                var statusResponse = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain"),
                };
                return Task.FromResult(statusResponse);
            });

        public static StubHttpMessageHandler Throwing(Exception thrownException) =>
            new StubHttpMessageHandler((request, cancellationToken) => throw thrownException);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.ReceivedRequests.Add(request);
            return await this.responder(request, cancellationToken);
        }
    }

    internal sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler messageHandler;
        private readonly Uri baseAddress;

        public StubHttpClientFactory(HttpMessageHandler messageHandler)
            : this(messageHandler, new Uri("http://api.test.local/"))
        {
        }

        public StubHttpClientFactory(HttpMessageHandler messageHandler, Uri baseAddress)
        {
            this.messageHandler = messageHandler;
            this.baseAddress = baseAddress;
        }

        public HttpClient CreateClient(string name)
        {
            var createdClient = new HttpClient(this.messageHandler, disposeHandler: false)
            {
                BaseAddress = this.baseAddress,
            };
            return createdClient;
        }
    }
}
