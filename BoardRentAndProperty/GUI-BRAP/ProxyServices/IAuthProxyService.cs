using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface IAuthProxyService
    {
        Task<AccountProfileDataTransferObject> LoginAsync(LoginDataTransferObject body, CancellationToken cancellationToken = default);

        Task RegisterAsync(RegisterDataTransferObject body, CancellationToken cancellationToken = default);

        Task LogoutAsync(CancellationToken cancellationToken = default);

        Task<string> ForgotPasswordAsync(CancellationToken cancellationToken = default);
    }
}
