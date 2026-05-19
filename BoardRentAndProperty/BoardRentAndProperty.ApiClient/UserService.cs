using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class UserService : ApiServiceBase, IUserService
    {
        public UserService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult<IReadOnlyList<UserDTO>>> GetUsersExceptAsync(Guid excludeAccountId, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<IReadOnlyList<UserDTO>>(
                token => client.GetAsync($"api/users/except/{excludeAccountId}", token),
                async (response, token) =>
                {
                    var result = await ApiResponseReader.ReadJsonAsync<List<UserDTO>>(response, token);
                    return result.Success
                        ? ServiceResult<IReadOnlyList<UserDTO>>.Ok(result.Data ?? new List<UserDTO>())
                        : ServiceResult<IReadOnlyList<UserDTO>>.Fail(result);
                },
                cancellationToken);
        }
    }
}
