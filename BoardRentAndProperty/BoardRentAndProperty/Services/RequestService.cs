using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public class RequestService : IRequestService
    {
        private readonly HttpClient httpClient;

        public RequestService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) =>
            FetchList($"api/requests/renter/{renterAccountId}");

        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) =>
            FetchList($"api/requests/owner/{ownerAccountId}");

        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) =>
            FetchList($"api/requests/owner/{ownerAccountId}/open");

        public Result<int, CreateRequestError> CreateRequest(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            var body = new CreateRequestDataTransferObject
            {
                GameId = gameId,
                RenterAccountId = renterAccountId,
                OwnerAccountId = ownerAccountId,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = this.httpClient.PostAsJsonAsync("api/requests", body).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var payload = response.Content.ReadFromJsonAsync<IdEnvelope>().GetAwaiter().GetResult();
                return Result<int, CreateRequestError>.Success(payload?.Id ?? 0);
            }

            string errorCode = ReadErrorEnvelope(response);
            return Result<int, CreateRequestError>.Failure(ParseEnum(errorCode, CreateRequestError.InvalidDateRange));
        }

        public Result<int, ApproveRequestError> ApproveRequest(int requestId, Guid ownerAccountId)
        {
            var body = new RequestActionDataTransferObject { AccountId = ownerAccountId };
            var response = this.httpClient.PutAsJsonAsync($"api/requests/{requestId}/approve", body).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var payload = response.Content.ReadFromJsonAsync<RentalIdEnvelope>().GetAwaiter().GetResult();
                return Result<int, ApproveRequestError>.Success(payload?.RentalId ?? 0);
            }

            return Result<int, ApproveRequestError>.Failure(MapApproveStatus(response.StatusCode));
        }

        public Result<int, DenyRequestError> DenyRequest(int requestId, Guid ownerAccountId, string denialReason)
        {
            var body = new RequestActionDataTransferObject { AccountId = ownerAccountId, Reason = denialReason };
            var response = this.httpClient.PutAsJsonAsync($"api/requests/{requestId}/deny", body).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                return Result<int, DenyRequestError>.Success(requestId);
            }

            return Result<int, DenyRequestError>.Failure(MapDenyStatus(response.StatusCode));
        }

        public Result<int, CancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId)
        {
            var body = new RequestActionDataTransferObject { AccountId = cancellingAccountId };
            var response = this.httpClient.PutAsJsonAsync($"api/requests/{requestId}/cancel", body).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                return Result<int, CancelRequestError>.Success(requestId);
            }

            return Result<int, CancelRequestError>.Failure(MapCancelStatus(response.StatusCode));
        }

        public Result<int, OfferError> OfferGame(int requestId, Guid offeringOwnerAccountId)
        {
            var body = new RequestActionDataTransferObject { AccountId = offeringOwnerAccountId };
            var response = this.httpClient.PutAsJsonAsync($"api/requests/{requestId}/offer", body).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var payload = response.Content.ReadFromJsonAsync<RentalIdEnvelope>().GetAwaiter().GetResult();
                return Result<int, OfferError>.Success(payload?.RentalId ?? 0);
            }

            return Result<int, OfferError>.Failure(MapOfferStatus(response.StatusCode));
        }

        public void OnGameDeactivated(int gameId)
        {
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            var query = $"api/requests/games/{gameId}/availability?startDate={Uri.EscapeDataString(startDate.ToString("o"))}&endDate={Uri.EscapeDataString(endDate.ToString("o"))}";
            var response = this.httpClient.GetAsync(query).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            return response.Content.ReadFromJsonAsync<bool>().GetAwaiter().GetResult();
        }

        public ImmutableList<(DateTime StartDate, DateTime EndDate)> GetBookedDates(int gameId, int calendarMonth, int calendarYear)
        {
            var query = $"api/requests/games/{gameId}/booked-dates?month={calendarMonth}&year={calendarYear}";
            var response = this.httpClient.GetAsync(query).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return ImmutableList<(DateTime StartDate, DateTime EndDate)>.Empty;
            }

            var list = response.Content.ReadFromJsonAsync<List<BookedDateRangeDataTransferObject>>().GetAwaiter().GetResult() ?? new List<BookedDateRangeDataTransferObject>();
            return list.Select(range => (range.StartDate, range.EndDate)).ToImmutableList();
        }

        private ImmutableList<RequestDTO> FetchList(string requestPath)
        {
            var response = this.httpClient.GetAsync(requestPath).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return ImmutableList<RequestDTO>.Empty;
            }

            var list = response.Content.ReadFromJsonAsync<List<RequestDTO>>().GetAwaiter().GetResult() ?? new List<RequestDTO>();
            return list.ToImmutableList();
        }

        private static ApproveRequestError MapApproveStatus(HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.NotFound => ApproveRequestError.NotFound,
                HttpStatusCode.Forbidden => ApproveRequestError.Unauthorized,
                _ => ApproveRequestError.TransactionFailed,
            };

        private static DenyRequestError MapDenyStatus(HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.NotFound => DenyRequestError.NotFound,
                HttpStatusCode.Forbidden => DenyRequestError.Unauthorized,
                _ => DenyRequestError.NotFound,
            };

        private static CancelRequestError MapCancelStatus(HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.NotFound => CancelRequestError.NotFound,
                HttpStatusCode.Forbidden => CancelRequestError.Unauthorized,
                _ => CancelRequestError.NotFound,
            };

        private static OfferError MapOfferStatus(HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.NotFound => OfferError.NotFound,
                HttpStatusCode.Forbidden => OfferError.NotOwner,
                _ => OfferError.TransactionFailed,
            };

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                return fallback;
            }

            return Enum.TryParse<TEnum>(value, out var parsed) ? parsed : fallback;
        }

        private static string ReadErrorEnvelope(HttpResponseMessage response)
        {
            try
            {
                var envelope = response.Content.ReadFromJsonAsync<ErrorEnvelope>().GetAwaiter().GetResult();
                return envelope?.Error ?? string.Empty;
            }
            catch (System.Text.Json.JsonException)
            {
                return string.Empty;
            }
            catch (NotSupportedException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
            catch (HttpRequestException)
            {
                return string.Empty;
            }
        }

        private sealed class ErrorEnvelope
        {
            public string? Error { get; set; }
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
