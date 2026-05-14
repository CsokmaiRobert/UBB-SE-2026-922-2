namespace GUI_BRAP.ProxyServices
{
    using System;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Contracts.DataTransferObjects;

    public interface IAccountProxyService
    {
        Task<AccountProfileDataTransferObject> GetProfileAsync(Guid accountId);
        Task UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject updateData);
        Task UploadAvatarAsync(Guid accountId, string imagePath);
        Task RemoveAvatarAsync(Guid accountId);
        Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword);
    }
}