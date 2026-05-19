using System;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public interface IAccountService
    {
        Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData, CancellationToken cancellationToken = default);

        Task<ServiceResult> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

        Task<ServiceResult<string>> UploadAvatarAsync(Guid accountId, string sourceFilePath, CancellationToken cancellationToken = default);

        Task<ServiceResult> RemoveAvatarAsync(Guid accountId, CancellationToken cancellationToken = default);
    }
}
