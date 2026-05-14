using GUI_BRAP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GUI_BRAP.ProxyServices
{

    public interface IAdminProxyService
    {
 
        Task<IEnumerable<AdminAccountViewModel>> GetAllAccountsAsync();

        Task SuspendAccountAsync(string accountId);

        Task UnsuspendAccountAsync(string accountId);

        Task ResetPasswordAsync(string accountId, string newPassword);
        Task UnlockAccountAsync(string accountId);
    }
}