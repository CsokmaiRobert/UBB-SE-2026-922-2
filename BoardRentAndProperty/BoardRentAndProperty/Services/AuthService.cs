using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly ISessionContext sessionContext;

        public AuthService(HttpClient httpClient, ISessionContext sessionContext)
        {
            this.httpClient = httpClient;
            this.sessionContext = sessionContext;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            var registerResponse = await this.httpClient.PostAsJsonAsync("api/auth/register", registrationRequest);
            if (!registerResponse.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.Fail(await ReadErrorAsync(registerResponse));
            }

            var loginAttempt = await this.LoginAsync(new LoginDataTransferObject
            {
                UsernameOrEmail = registrationRequest.Username,
                Password = registrationRequest.Password,
            });

            return loginAttempt.Success ? ServiceResult<bool>.Ok(true) : ServiceResult<bool>.Fail(loginAttempt.Error ?? "Registration succeeded but auto-login failed.");
        }

        public async Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject loginRequest)
        {
            var loginResponse = await this.httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            if (!loginResponse.IsSuccessStatusCode)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail(await ReadErrorAsync(loginResponse));
            }

            var profile = await loginResponse.Content.ReadFromJsonAsync<AccountProfileDataTransferObject>();
            if (profile == null)
            {
                return ServiceResult<AccountProfileDataTransferObject>.Fail("Login response was empty.");
            }

            ApiUrlHelper.RebaseAvatarUrl(this.httpClient.BaseAddress!, profile);
            this.sessionContext.Populate(profile);
            return ServiceResult<AccountProfileDataTransferObject>.Ok(profile);
        }

        public async Task<ServiceResult<bool>> LogoutAsync()
        {
            this.sessionContext.Clear();
            await this.httpClient.PostAsync("api/auth/logout", content: null);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync()
        {
            var forgotResponse = await this.httpClient.GetAsync("api/auth/forgot-password");
            if (!forgotResponse.IsSuccessStatusCode)
            {
                return ServiceResult<string>.Fail(await ReadErrorAsync(forgotResponse));
            }

            string? message = await forgotResponse.Content.ReadFromJsonAsync<string>();
            return ServiceResult<string>.Ok(message ?? string.Empty);
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var errorEnvelope = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
                if (!string.IsNullOrEmpty(errorEnvelope?.Error))
                {
                    return errorEnvelope!.Error!;
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
