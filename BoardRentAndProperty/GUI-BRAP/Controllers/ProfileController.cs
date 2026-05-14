namespace GUI_BRAP.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Contracts.DataTransferObjects;
    using GUI_BRAP.Authorization;
    using GUI_BRAP.Infrastructure;
    using GUI_BRAP.Models;
    using GUI_BRAP.ProxyServices;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAccountProxyService accountProxyService;
        private readonly IConfiguration configuration;

        public ProfileController(IAccountProxyService accountProxyService, IConfiguration configuration)
        {
            this.accountProxyService = accountProxyService ?? throw new ArgumentNullException(nameof(accountProxyService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                Guid currentUserId = User.GetAccountId();
                AccountProfileDataTransferObject profileData = await this.accountProxyService.GetProfileAsync(currentUserId);

                string fullAvatarUrl = string.Empty;
                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    string apiBaseUrl = this.configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
                    string avatarPath = profileData.AvatarUrl.TrimStart('/');

                    if (!avatarPath.StartsWith("avatars/"))
                    {
                        avatarPath = $"avatars/{avatarPath}";
                    }

                    fullAvatarUrl = $"{apiBaseUrl}/{avatarPath}";
                }

                ProfileViewModel model = new ProfileViewModel
                {
                    Username = profileData.Username,
                    DisplayName = profileData.DisplayName ?? string.Empty,
                    Email = profileData.Email ?? string.Empty,
                    PhoneNumber = profileData.PhoneNumber,
                    Country = profileData.Country,
                    City = profileData.City,
                    StreetName = profileData.StreetName,
                    StreetNumber = profileData.StreetNumber,
                    AvatarUrl = fullAvatarUrl
                };

                return this.View(model);
            }
            catch (ProxyServiceException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
                return this.RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                Guid currentUserId = User.GetAccountId();
                var profileData = await this.accountProxyService.GetProfileAsync(currentUserId);

                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    string apiBaseUrl = this.configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
                    string fileName = Path.GetFileName(profileData.AvatarUrl);
                    model.AvatarUrl = $"{apiBaseUrl}/avatars/{fileName}";
                }

                return this.View(model);
            }

            try
            {
                Guid currentUserId = User.GetAccountId();
                AccountProfileDataTransferObject updateData = new AccountProfileDataTransferObject
                {
                    DisplayName = model.DisplayName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Country = model.Country,
                    City = model.City,
                    StreetName = model.StreetName,
                    StreetNumber = model.StreetNumber
                };

                await this.accountProxyService.UpdateProfileAsync(currentUserId, updateData);
                this.TempData["SuccessMessage"] = "Profile updated successfully.";
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ProxyServiceException exception)
            {
                Guid currentUserId = User.GetAccountId();
                var profileData = await this.accountProxyService.GetProfileAsync(currentUserId);
                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    string apiBaseUrl = this.configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
                    model.AvatarUrl = $"{apiBaseUrl}/avatars/{Path.GetFileName(profileData.AvatarUrl)}";
                }

                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(ProfileViewModel model)
        {
            if (model.AvatarFile == null || model.AvatarFile.Length == 0)
            {
                this.TempData["ErrorMessage"] = "Please select a valid image file.";
                return this.RedirectToAction(nameof(this.Index));
            }

            try
            {
                Guid currentUserId = User.GetAccountId();

                string extension = Path.GetExtension(model.AvatarFile.FileName);
                string temporaryPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

                using (FileStream stream = new FileStream(temporaryPath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                await this.accountProxyService.UploadAvatarAsync(currentUserId, temporaryPath);

                if (System.IO.File.Exists(temporaryPath))
                {
                    System.IO.File.Delete(temporaryPath);
                }

                this.TempData["SuccessMessage"] = "Avatar uploaded successfully.";
            }
            catch (Exception exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAvatar()
        {
            try
            {
                Guid currentUserId = User.GetAccountId();
                await this.accountProxyService.RemoveAvatarAsync(currentUserId);
                this.TempData["SuccessMessage"] = "Avatar removed successfully.";
            }
            catch (ProxyServiceException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return this.View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                Guid currentUserId = User.GetAccountId();
                await this.accountProxyService.ChangePasswordAsync(currentUserId, model.CurrentPassword, model.NewPassword);

                this.TempData["SuccessMessage"] = "Password changed successfully. Please login again.";
                return this.RedirectToAction("Login", "Auth");
            }
            catch (ProxyServiceException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }
        }
    }
}