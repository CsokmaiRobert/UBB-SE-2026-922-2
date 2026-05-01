using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public class RentalService : IRentalService
    {
        private readonly HttpClient httpClient;

        public RentalService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) =>
            FetchList($"api/rentals/renter/{renterAccountId}");

        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) =>
            FetchList($"api/rentals/owner/{ownerAccountId}");

        public bool IsSlotAvailable(int gameId, DateTime startDate, DateTime endDate)
        {
            var query = $"api/rentals/games/{gameId}/availability?startDate={Uri.EscapeDataString(startDate.ToString("o"))}&endDate={Uri.EscapeDataString(endDate.ToString("o"))}";
            var response = this.httpClient.GetAsync(query).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            return response.Content.ReadFromJsonAsync<bool>().GetAwaiter().GetResult();
        }

        public void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            var body = new CreateRentalDataTransferObject
            {
                GameId = gameId,
                RenterAccountId = renterAccountId,
                OwnerAccountId = ownerAccountId,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = this.httpClient.PostAsJsonAsync("api/rentals", body).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string errorMessage = ReadErrorEnvelope(response);
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException(string.IsNullOrEmpty(errorMessage) ? "Rental conflict." : errorMessage);
            }

            throw new ArgumentException(string.IsNullOrEmpty(errorMessage) ? "Rental creation failed." : errorMessage);
        }

        private ImmutableList<RentalDTO> FetchList(string requestPath)
        {
            var response = this.httpClient.GetAsync(requestPath).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return ImmutableList<RentalDTO>.Empty;
            }

            var list = response.Content.ReadFromJsonAsync<List<RentalDTO>>().GetAwaiter().GetResult() ?? new List<RentalDTO>();
            return list.ToImmutableList();
        }

        private static string ReadErrorEnvelope(HttpResponseMessage response)
        {
            try
            {
                var envelope = response.Content.ReadFromJsonAsync<ErrorEnvelope>().GetAwaiter().GetResult();
                return envelope?.Error ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private sealed class ErrorEnvelope
        {
            public string? Error { get; set; }
        }
    }
}
