using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class RequestService : ApiServiceBase, IRequestService
    {
        public RequestService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/requests/renter/{renterAccountId}", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/requests/owner/{ownerAccountId}", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<RequestDTO>>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/requests/owner/{ownerAccountId}/open", cancellationToken);

        public Task<ServiceResult<int>> CreateRequestAsync(CreateRequestDataTransferObject request, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<int>(
                token => client.PostAsJsonAsync("api/requests", request, token),
                (response, token) => ReadIdAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<int>> ApproveRequestAsync(int requestId, Guid ownerAccountId, CancellationToken cancellationToken = default)
        {
            var body = new RequestActionDataTransferObject { AccountId = ownerAccountId };
            var client = CreateClient();
            return ApiResponseReader.SendAsync<int>(
                token => client.PutAsJsonAsync($"api/requests/{requestId}/approve", body, token),
                (response, token) => ReadRentalIdAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<int>> DenyRequestAsync(int requestId, RequestActionDataTransferObject action, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<int>(
                token => client.PutAsJsonAsync($"api/requests/{requestId}/deny", action, token),
                (response, token) => ValueOrFailAsync(response, requestId, token),
                cancellationToken);
        }

        public Task<ServiceResult<int>> CancelRequestAsync(int requestId, RequestActionDataTransferObject action, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<int>(
                token => client.PutAsJsonAsync($"api/requests/{requestId}/cancel", action, token),
                (response, token) => ValueOrFailAsync(response, requestId, token),
                cancellationToken);
        }

        public Task<ServiceResult<int>> OfferGameAsync(int requestId, RequestActionDataTransferObject action, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<int>(
                token => client.PutAsJsonAsync($"api/requests/{requestId}/offer", action, token),
                (response, token) => ReadRentalIdAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<bool>> CheckAvailabilityAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var query = $"api/requests/games/{gameId}/availability?startDate={Uri.EscapeDataString(startDate.ToString("o"))}&endDate={Uri.EscapeDataString(endDate.ToString("o"))}";
            var client = CreateClient();
            return ApiResponseReader.SendAsync<bool>(
                token => client.GetAsync(query, token),
                (response, token) => ApiResponseReader.ReadJsonAsync<bool>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>> GetBookedDatesAsync(int gameId, int calendarMonth, int calendarYear, CancellationToken cancellationToken = default)
        {
            var query = $"api/requests/games/{gameId}/booked-dates?month={calendarMonth}&year={calendarYear}";
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<BookedDateRangeDataTransferObject>>(
                token => client.GetAsync(query, token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<BookedDateRangeDataTransferObject>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>.Ok(parsed.Data ?? new List<BookedDateRangeDataTransferObject>())
                        : ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>.Fail(parsed);
                },
                cancellationToken);
        }

        private Task<ServiceResult<IReadOnlyList<RequestDTO>>> FetchListAsync(string requestPath, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<RequestDTO>>(
                token => client.GetAsync(requestPath, token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<RequestDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<RequestDTO>>.Ok(parsed.Data ?? new List<RequestDTO>())
                        : ServiceResult<IReadOnlyList<RequestDTO>>.Fail(parsed);
                },
                cancellationToken);
        }

        private static async Task<ServiceResult<int>> ReadIdAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                return await ApiResponseReader.ToFailAsync<int>(response, cancellationToken);
            }

            var parsed = await ApiResponseReader.ReadJsonAsync<IdEnvelope>(response, cancellationToken);
            return parsed.Success ? ServiceResult<int>.Ok(parsed.Data?.Id ?? 0) : ServiceResult<int>.Fail(parsed);
        }

        private static async Task<ServiceResult<int>> ReadRentalIdAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                return await ApiResponseReader.ToFailAsync<int>(response, cancellationToken);
            }

            var parsed = await ApiResponseReader.ReadJsonAsync<RentalIdEnvelope>(response, cancellationToken);
            return parsed.Success ? ServiceResult<int>.Ok(parsed.Data?.RentalId ?? 0) : ServiceResult<int>.Fail(parsed);
        }

        private static async Task<ServiceResult<int>> ValueOrFailAsync(HttpResponseMessage response, int valueOnSuccess, CancellationToken cancellationToken)
        {
            return response.IsSuccessStatusCode
                ? ServiceResult<int>.Ok(valueOnSuccess)
                : await ApiResponseReader.ToFailAsync<int>(response, cancellationToken);
        }

        private sealed class IdEnvelope
        {
            public int Id { get; set; }
        }

        private sealed class RentalIdEnvelope
        {
            public int RentalId { get; set; }
        }
    }
}
