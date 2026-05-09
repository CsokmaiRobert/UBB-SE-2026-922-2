using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GUI_BRAP.Services
{
    internal sealed class ApiErrorEnvelope
    {
        public string? Error { get; set; }
    }

    internal static class HttpResponseEnsurer
    {
        public static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallbackMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string? message = null;
            try
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
                message = envelope?.Error;
            }
            catch
            {
            }

            throw new ProxyServiceException(string.IsNullOrEmpty(message) ? fallbackMessage : message, (int)response.StatusCode);
        }
    }
}
