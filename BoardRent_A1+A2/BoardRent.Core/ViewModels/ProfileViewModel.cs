namespace BoardRent.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using CommunityToolkit.Mvvm.Input;

    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;
        private readonly IFilePickerService filePickerService;
        private readonly ISessionContext sessionContext;

        private string pendingAvatarPath = string.Empty;
        private string username = string.Empty;
        private string displayName = string.Empty;
        private string email = string.Empty;
        private string phoneNumber = string.Empty;
        private string country = string.Empty;
        private string city = string.Empty;
        private string streetName = string.Empty;
        private string streetNumber = string.Empty;
        private string avatarUrl = string.Empty;
        private string currentPassword = string.Empty;
        private string newPassword = string.Empty;
        private string confirmPassword = string.Empty;
        private string confirmPasswordError = string.Empty;

        public ProfileViewModel(
            IUserService userService,
            IAuthService authService,
            IFilePickerService filePickerService,
            ISessionContext sessionContext)
        {
            this.userService = userService;
            this.authService = authService;
            this.filePickerService = filePickerService;
            this.sessionContext = sessionContext;

            this.SaveProfileCommand = new AsyncRelayCommand(this.SaveProfileAsync);
            this.RemoveAvatarCommand = new AsyncRelayCommand(this.RemoveAvatarAsync);
            this.SelectAvatarCommand = new AsyncRelayCommand(this.SelectAvatarAsync);
            this.SaveNewPasswordCommand = new AsyncRelayCommand(this.SaveNewPasswordAsync);
            this.SignOutCommand = new AsyncRelayCommand(this.SignOutAsync);
        }

        public Action OnSignOutSuccess { get; set; }

        public string Username { get => this.username; set => this.SetProperty(ref this.username, value); }
        public string DisplayName { get => this.displayName; set => this.SetProperty(ref this.displayName, value); }
        public string Email { get => this.email; set => this.SetProperty(ref this.email, value); }
        public string PhoneNumber { get => this.phoneNumber; set => this.SetProperty(ref this.phoneNumber, value); }
        public string Country { get => this.country; set => this.SetProperty(ref this.country, value); }
        public string City { get => this.city; set => this.SetProperty(ref this.city, value); }
        public string StreetName { get => this.streetName; set => this.SetProperty(ref this.streetName, value); }
        public string StreetNumber { get => this.streetNumber; set => this.SetProperty(ref this.streetNumber, value); }
        public string AvatarUrl { get => this.avatarUrl; set => this.SetProperty(ref this.avatarUrl, value); }
        public string CurrentPassword { get => this.currentPassword; set => this.SetProperty(ref this.currentPassword, value); }
        public string NewPassword { get => this.newPassword; set => this.SetProperty(ref this.newPassword, value); }
        public string ConfirmPassword { get => this.confirmPassword; set => this.SetProperty(ref this.confirmPassword, value); }
        public string ConfirmPasswordError { get => this.confirmPasswordError; set => this.SetProperty(ref this.confirmPasswordError, value); }

        public ICommand SaveProfileCommand { get; }
        public ICommand SelectAvatarCommand { get; }
        public ICommand RemoveAvatarCommand { get; }
        public ICommand SaveNewPasswordCommand { get; }
        public ICommand SignOutCommand { get; }

        public async Task LoadProfileAsync()
        {
            this.IsLoading = true;
            Guid currentUserId = this.sessionContext.UserId;

            ServiceResult<UserProfileDataTransferObject> profileResult = await this.userService.GetProfileAsync(currentUserId);

            if (profileResult.Success && profileResult.Data != null)
            {
                this.Username = profileResult.Data.Username;
                this.DisplayName = profileResult.Data.DisplayName;
                this.Email = profileResult.Data.Email;
                this.PhoneNumber = profileResult.Data.PhoneNumber;
                this.Country = profileResult.Data.Country;
                this.City = profileResult.Data.City;
                this.StreetName = profileResult.Data.StreetName;
                this.StreetNumber = profileResult.Data.StreetNumber;
                this.AvatarUrl = profileResult.Data.AvatarUrl;
            }

            this.IsLoading = false;
        }

        private async Task SaveProfileAsync()
        {
            this.IsLoading = true;
            Guid currentUserId = this.sessionContext.UserId;

            UserProfileDataTransferObject updateInformation = new UserProfileDataTransferObject
            {
                DisplayName = this.DisplayName,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                Country = this.Country,
                City = this.City,
                StreetName = this.StreetName,
                StreetNumber = this.StreetNumber
            };

            ServiceResult<bool> updateResult = await this.userService.UpdateProfileAsync(currentUserId, updateInformation);

            if (updateResult.Success)
            {
                if (!string.IsNullOrEmpty(this.pendingAvatarPath))
                {
                    this.AvatarUrl = await this.userService.UploadAvatarAsync(currentUserId, this.pendingAvatarPath);
                    this.pendingAvatarPath = string.Empty;
                }
                this.ErrorMessage = "Profile saved successfully.";
            }
            else
            {
                this.ErrorMessage = updateResult.Error;
            }

            this.IsLoading = false;
        }

        private async Task SelectAvatarAsync()
        {
            string selectedFilePath = await this.filePickerService.PickImageFileAsync();
            if (selectedFilePath != null)
            {
                this.pendingAvatarPath = selectedFilePath;
                this.AvatarUrl = selectedFilePath;
            }
        }

        private async Task RemoveAvatarAsync()
        {
            await this.userService.RemoveAvatarAsync(this.sessionContext.UserId);
            this.AvatarUrl = string.Empty;
            this.pendingAvatarPath = string.Empty;
        }

        private async Task SaveNewPasswordAsync()
        {
            this.ConfirmPasswordError = string.Empty;

            if (this.NewPassword != this.ConfirmPassword)
            {
                this.ConfirmPasswordError = "Passwords do not match";
                return;
            }

            ServiceResult<bool> passwordChangeResult = await this.userService.ChangePasswordAsync(
                this.sessionContext.UserId,
                this.CurrentPassword,
                this.NewPassword);

            if (passwordChangeResult.Success)
            {
                this.ErrorMessage = "Password updated. Redirecting to login...";
                await Task.Delay(2000);
                await this.SignOutAsync();
            }
            else
            {
                this.ErrorMessage = passwordChangeResult.Error;
            }
        }

        private async Task SignOutAsync()
        {
            await this.authService.LogoutAsync();
            this.OnSignOutSuccess?.Invoke();
        }
    }
}