using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class RentalService : ApiServiceBase, IRentalService
    {
        public RentalService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/rentals/renter/{renterAccountId}", cancellationToken);

        public Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => FetchListAsync($"api/rentals/owner/{ownerAccountId}", cancellationToken);

        public Task<ServiceResult<bool>> IsSlotAvailableAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var query = $"api/rentals/games/{gameId}/availability?startDate={Uri.EscapeDataString(startDate.ToString("o"))}&endDate={Uri.EscapeDataString(endDate.ToString("o"))}";
            var client = CreateClient();
            return ApiResponseReader.SendAsync<bool>(
                token => client.GetAsync(query, token),
                (response, token) => ApiResponseReader.ReadJsonAsync<bool>(response, token),
                cancellationToken);
        }

        public Task<ServiceResult> CreateConfirmedRentalAsync(CreateRentalDataTransferObject rental, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsJsonAsync("api/rentals", rental, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        private Task<ServiceResult<IReadOnlyList<RentalDTO>>> FetchListAsync(string requestPath, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<RentalDTO>>(
                token => client.GetAsync(requestPath, token),
                async (response, token) =>
                {
                    var parsed = await ApiResponseReader.ReadJsonAsync<List<RentalDTO>>(response, token);
                    return parsed.Success
                        ? ServiceResult<IReadOnlyList<RentalDTO>>.Ok(parsed.Data ?? new List<RentalDTO>())
                        : ServiceResult<IReadOnlyList<RentalDTO>>.Fail(parsed);
                },
                cancellationToken);
        }
    }
}
