using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BoardRentAndProperty.ApiClient
{
    internal static class ApiResponseReader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task<ServiceResult<T>> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                return await ToFailAsync<T>(response, cancellationToken);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            T? value;
            try
            {
                value = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
            }
            catch (JsonException exception)
            {
                return ServiceResult<T>.Fail($"Could not read the API response. {exception.Message}", response.StatusCode);
            }

            if (value is null)
            {
                return ServiceResult<T>.Fail("The API returned an empty response body.", response.StatusCode);
            }

            return ServiceResult<T>.Ok(value);
        }

        public static async Task<ServiceResult<string>> ReadStringAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                return await ToFailAsync<string>(response, cancellationToken);
            }

            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ServiceResult<string>.Ok(body);
        }

        public static async Task<ServiceResult> EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            return response.IsSuccessStatusCode
                ? ServiceResult.Ok()
                : await ToFailAsync(response, cancellationToken);
        }

        public static async Task<ServiceResult> ToFailAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var (message, code) = await ParseErrorAsync(response, cancellationToken);
            return ServiceResult.Fail(message, response.StatusCode, code);
        }

        public static async Task<ServiceResult<T>> ToFailAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var (message, code) = await ParseErrorAsync(response, cancellationToken);
            return ServiceResult<T>.Fail(message, response.StatusCode, code);
        }

        public static ServiceResult FromException(Exception exception)
            => ServiceResult.Fail(DescribeTransportFault(exception), TransportStatus(exception));

        public static ServiceResult<T> FromException<T>(Exception exception)
            => ServiceResult<T>.Fail(DescribeTransportFault(exception), TransportStatus(exception));

        public static async Task<ServiceResult<T>> SendAsync<T>(
            Func<CancellationToken, Task<HttpResponseMessage>> send,
            Func<HttpResponseMessage, CancellationToken, Task<ServiceResult<T>>> interpret,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await send(cancellationToken);
                return await interpret(response, cancellationToken);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                return FromException<T>(exception);
            }
        }

        public static async Task<ServiceResult> SendAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> send,
            Func<HttpResponseMessage, CancellationToken, Task<ServiceResult>> interpret,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await send(cancellationToken);
                return await interpret(response, cancellationToken);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                return FromException(exception);
            }
        }

        private static async Task<(string Message, string? Code)> ParseErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string body = string.Empty;
            try
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch
            {
                body = string.Empty;
            }

            string message = $"API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).";
            string? code = null;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var envelope = JsonSerializer.Deserialize<ApiErrorEnvelope>(body, JsonOptions);
                    if (envelope is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(envelope.Error))
                        {
                            message = envelope.Error!;
                        }

                        code = envelope.Code;
                    }
                }
                catch (JsonException)
                {
                }
            }

            return (message, code);
        }

        private static string DescribeTransportFault(Exception exception)
            => exception is TaskCanceledException or OperationCanceledException
                ? "The API did not respond in time. Check that the API server is running and reachable."
                : "Cannot connect to the API. Check that the API server is running and reachable.";

        private static HttpStatusCode TransportStatus(Exception exception)
            => exception is TaskCanceledException or OperationCanceledException
                ? HttpStatusCode.RequestTimeout
                : HttpStatusCode.ServiceUnavailable;

        private sealed class ApiErrorEnvelope
        {
            public string? Error { get; set; }

            public string? Code { get; set; }
        }
    }
}
