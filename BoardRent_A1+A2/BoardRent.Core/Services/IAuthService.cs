using System.Threading.Tasks;
using BoardRent.DataTransferObjects;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject dto);

        Task<ServiceResult<UserProfileDataTransferObject>> LoginAsync(LoginDataTransferObject dto);

        Task<ServiceResult<bool>> LogoutAsync();

        Task<ServiceResult<string>> ForgotPasswordAsync();
    }
}


