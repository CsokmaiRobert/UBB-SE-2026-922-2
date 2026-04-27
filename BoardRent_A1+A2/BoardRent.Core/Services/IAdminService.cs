using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRent.DataTransferObjects;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<List<UserProfileDataTransferObject>>> GetAllUsersAsync(int page, int pageSize);
        Task<ServiceResult<bool>> SuspendUserAsync(Guid userId);
        Task<ServiceResult<bool>> UnsuspendUserAsync(Guid userId);
        Task<ServiceResult<bool>> ResetPasswordAsync(Guid userId, string newPassword);
        Task<ServiceResult<bool>> UnlockAccountAsync(Guid userId);
    }
}
