using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GUI_BRAP.Infrastructure
{
    public static class HttpResponseEnsurer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string body = string.Empty;
            try
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch
            {
            }

            string message = $"API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).";
            string? apiErrorCode = null;

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

                        apiErrorCode = envelope.Code;
                    }
                }
                catch (JsonException)
                {
                }
            }

            throw new ProxyServiceException(message, response.StatusCode, apiErrorCode);
        }

        public static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            await EnsureSuccessAsync(response, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var value = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
            if (value is null)
            {
                throw new ProxyServiceException(
                    "API returned an empty response body.",
                    response.StatusCode,
                    apiErrorCode: null);
            }

            return value;
        }
    }
}
