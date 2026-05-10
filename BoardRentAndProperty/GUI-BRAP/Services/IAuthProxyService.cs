using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.Services
{
    public interface IAuthProxyService
    {
        Task<AccountProfileDataTransferObject> LoginAsync(LoginDataTransferObject body);
    }
}
