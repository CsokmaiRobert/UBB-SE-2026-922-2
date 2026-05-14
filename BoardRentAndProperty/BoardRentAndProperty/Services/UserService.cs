using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient httpClient;

        public UserService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId)
        {
            var response = this.httpClient.GetAsync($"api/users/except/{excludeAccountId}").GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return ImmutableList<UserDTO>.Empty;
            }

            var list = response.Content.ReadFromJsonAsync<List<UserDTO>>().GetAwaiter().GetResult() ?? new List<UserDTO>();
            return list.ToImmutableList();
        }
    }
}
