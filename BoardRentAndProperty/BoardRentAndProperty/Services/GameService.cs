using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public class GameService : IGameService
    {
        private const int NoValidationErrors = 0;

        private readonly HttpClient httpClient;

        public GameService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public List<string> ValidateGame(GameDTO gameDto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(gameDto.Name)
                || gameDto.Name.Length < DomainConstants.GameMinimumNameLength
                || gameDto.Name.Length > DomainConstants.GameMaximumNameLength)
            {
                errors.Add($"Name must be between {DomainConstants.GameMinimumNameLength} and {DomainConstants.GameMaximumNameLength} characters.");
            }

            if (gameDto.Price < DomainConstants.GameMinimumAllowedPrice)
            {
                errors.Add($"Price must be greater than or equal to {DomainConstants.GameMinimumAllowedPrice:0}.");
            }

            if (gameDto.MinimumPlayerNumber < DomainConstants.GameMinimumPlayerCount)
            {
                errors.Add($"Minimum player count must be at least {DomainConstants.GameMinimumPlayerCount}.");
            }

            if (gameDto.MaximumPlayerNumber < gameDto.MinimumPlayerNumber)
            {
                errors.Add("Maximum player count must be greater than or equal to minimum player count.");
            }

            if (string.IsNullOrWhiteSpace(gameDto.Description)
                || gameDto.Description.Length < DomainConstants.GameMinimumDescriptionLength
                || gameDto.Description.Length > DomainConstants.GameMaximumDescriptionLength)
            {
                errors.Add($"Description must be between {DomainConstants.GameMinimumDescriptionLength} and {DomainConstants.GameMaximumDescriptionLength} characters.");
            }

            return errors;
        }

        public void AddGame(GameDTO gameToAdd)
        {
            var errors = ValidateGame(gameToAdd);
            if (errors.Count > NoValidationErrors)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            var response = this.httpClient.PostAsJsonAsync("api/games", gameToAdd).GetAwaiter().GetResult();
            EnsureSuccess(response, "Failed to create game.");
        }

        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameData)
        {
            var errors = ValidateGame(updatedGameData);
            if (errors.Count > NoValidationErrors)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            var response = this.httpClient.PutAsJsonAsync($"api/games/{gameId}", updatedGameData).GetAwaiter().GetResult();
            EnsureSuccess(response, "Failed to update game.");
        }

        public GameDTO DeleteGameByIdentifier(int gameId)
        {
            var response = this.httpClient.DeleteAsync($"api/games/{gameId}").GetAwaiter().GetResult();
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException(ReadErrorEnvelope(response));
            }

            EnsureSuccess(response, "Failed to delete game.");
            return response.Content.ReadFromJsonAsync<GameDTO>().GetAwaiter().GetResult() ?? new GameDTO { Id = gameId };
        }

        public GameDTO GetGameByIdentifier(int gameId)
        {
            var response = this.httpClient.GetAsync($"api/games/{gameId}").GetAwaiter().GetResult();
            EnsureSuccess(response, "Failed to fetch game.");
            return response.Content.ReadFromJsonAsync<GameDTO>().GetAwaiter().GetResult() ?? new GameDTO();
        }

        public ImmutableList<GameDTO> GetGamesForOwner(Guid ownerAccountId) =>
            FetchList($"api/games/owner/{ownerAccountId}");

        public ImmutableList<GameDTO> GetAllGames() =>
            FetchList("api/games");

        public ImmutableList<GameDTO> GetAvailableGamesForRenter(Guid renterAccountId) =>
            FetchList($"api/games/renter/{renterAccountId}/available");

        public ImmutableList<GameDTO> GetActiveGamesForOwner(Guid ownerAccountId) =>
            FetchList($"api/games/owner/{ownerAccountId}/active");

        private ImmutableList<GameDTO> FetchList(string requestPath)
        {
            var response = this.httpClient.GetAsync(requestPath).GetAwaiter().GetResult();
            EnsureSuccess(response, "Failed to fetch games.");
            var list = response.Content.ReadFromJsonAsync<List<GameDTO>>().GetAwaiter().GetResult() ?? new List<GameDTO>();
            return list.ToImmutableList();
        }

        private static void EnsureSuccess(HttpResponseMessage response, string genericMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string errorMessage = ReadErrorEnvelope(response);
            throw new InvalidOperationException(string.IsNullOrEmpty(errorMessage) ? genericMessage : errorMessage);
        }

        private static string ReadErrorEnvelope(HttpResponseMessage response)
        {
            try
            {
                var envelope = response.Content.ReadFromJsonAsync<ErrorEnvelope>().GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(envelope?.Error))
                {
                    return envelope!.Error!;
                }
            }
            catch
            {
            }

            return $"Server returned status {(int)response.StatusCode}.";
        }

        private sealed class ErrorEnvelope
        {
            public string? Error { get; set; }
        }
    }
}
