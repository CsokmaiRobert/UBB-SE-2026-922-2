using System;
using System.Threading.Tasks;
using BoardRent.DataTransferObjects;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserProfileDataTransferObject>> GetProfileAsync(Guid userId);
        Task<ServiceResult<bool>> UpdateProfileAsync(Guid userId, UserProfileDataTransferObject profileUpdateData);
        Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<string> UploadAvatarAsync(Guid userId, string sourceFilePath);
        Task RemoveAvatarAsync(Guid userId);
    }
}