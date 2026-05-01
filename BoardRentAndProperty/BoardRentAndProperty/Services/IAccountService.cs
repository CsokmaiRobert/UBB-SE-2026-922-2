using System;
using System.Threading.Tasks;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Services
{
    public interface IAccountService
    {
        Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId);
        Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData);
        Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword);
        Task<string> UploadAvatarAsync(Guid accountId, string sourceFilePath);
        Task RemoveAvatarAsync(Guid accountId);
    }
}
