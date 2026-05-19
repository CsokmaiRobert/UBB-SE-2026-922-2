using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;

namespace GUI_BRAP.Infrastructure
{
    public sealed class AuthProxyServiceAdapter : IAuthProxyService
    {
        private readonly IAuthService authService;

        public AuthProxyServiceAdapter(IAuthService authService)
        {
            this.authService = authService;
        }

        public async Task<AccountProfileDataTransferObject> LoginAsync(LoginDataTransferObject body, CancellationToken cancellationToken = default)
            => (await this.authService.LoginAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task RegisterAsync(RegisterDataTransferObject body, CancellationToken cancellationToken = default)
            => (await this.authService.RegisterAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
            => (await this.authService.LogoutAsync(cancellationToken)).ThrowIfFailed();

        public async Task<string> ForgotPasswordAsync(CancellationToken cancellationToken = default)
            => (await this.authService.ForgotPasswordAsync(cancellationToken)).ThrowIfFailed();
    }
}
