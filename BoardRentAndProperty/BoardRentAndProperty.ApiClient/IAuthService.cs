using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDataTransferObject request, CancellationToken cancellationToken = default);

        Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject request, CancellationToken cancellationToken = default);

        Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default);
    }
}
