using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class AuthService : ApiServiceBase, IAuthService
    {
        public AuthService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult> RegisterAsync(RegisterDataTransferObject request, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsJsonAsync("api/auth/register", request, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject request, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<AccountProfileDataTransferObject>(
                token => client.PostAsJsonAsync("api/auth/login", request, token),
                async (response, token) =>
                {
                    var result = await ApiResponseReader.ReadJsonAsync<AccountProfileDataTransferObject>(response, token);
                    if (result.Success && result.Data is not null)
                    {
                        ApiUrlHelper.RebaseAvatarUrl(client.BaseAddress!, result.Data);
                    }

                    return result;
                },
                cancellationToken);
        }

        public Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsync("api/auth/logout", content: null, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<string>(
                token => client.GetAsync("api/auth/forgot-password", token),
                (response, token) => ApiResponseReader.ReadStringAsync(response, token),
                cancellationToken);
        }
    }
}
