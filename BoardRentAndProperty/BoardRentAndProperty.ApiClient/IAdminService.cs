using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public interface IAdminService
    {
        Task<ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<ServiceResult> SuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> UnsuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> ResetPasswordAsync(Guid accountId, string newPassword, CancellationToken cancellationToken = default);

        Task<ServiceResult> UnlockAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    }
}
