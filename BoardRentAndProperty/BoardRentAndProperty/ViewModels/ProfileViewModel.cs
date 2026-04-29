namespace BoardRentAndProperty.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Services;
    using BoardRentAndProperty.Utilities;
    using CommunityToolkit.Mvvm.Input;

    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IAccountService accountService;
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
        private bool isAdmin;

        private string emailError = string.Empty;
        private string displayNameError = string.Empty;
        private string phoneError = string.Empty;
        private string streetNumberError = string.Empty;

        public string EmailError { get => this.emailError; set => this.SetProperty(ref this.emailError, value); }
        public string DisplayNameError { get => this.displayNameError; set => this.SetProperty(ref this.displayNameError, value); }
        public string PhoneError { get => this.phoneError; set => this.SetProperty(ref this.phoneError, value); }
        public string StreetNumberError { get => this.streetNumberError; set => this.SetProperty(ref this.streetNumberError, value); }

        public Microsoft.UI.Xaml.Visibility AdminButtonVisibility =>
        this.isAdmin ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

        public ProfileViewModel(
            IAccountService accountService,
            IAuthService authService,
            IFilePickerService filePickerService,
            ISessionContext sessionContext)
        {
            this.accountService = accountService;
            this.authService = authService;
            this.filePickerService = filePickerService;
            this.sessionContext = sessionContext;

            this.AvailableCountries.Add("Romania");
            this.AvailableCountries.Add("Germany");
            this.AvailableCountries.Add("France");

            this.SaveProfileCommand = new AsyncRelayCommand(this.SaveProfileAsync);
            this.RemoveAvatarCommand = new AsyncRelayCommand(this.RemoveAvatarAsync);
            this.SelectAvatarCommand = new AsyncRelayCommand(this.SelectAvatarAsync);
            this.SaveNewPasswordCommand = new AsyncRelayCommand(this.SaveNewPasswordAsync);
            this.SignOutCommand = new AsyncRelayCommand(this.SignOutAsync);
        }

        public Action OnSignOutSuccess { get; set; }

        public System.Collections.ObjectModel.ObservableCollection<string> AvailableCountries { get; } = new();

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

            this.PhoneNumber = this.sessionContext.PhoneNumber;
            this.City = this.sessionContext.City;
            this.Country = this.sessionContext.Country;
            this.StreetName = this.sessionContext.StreetName;
            this.StreetNumber = this.sessionContext.StreetNumber;

            this.Username = this.sessionContext.Username;
            this.DisplayName = this.sessionContext.DisplayName;
            this.Email = this.sessionContext.Email;
            this.PhoneNumber = this.sessionContext.PhoneNumber;
            this.Country = this.sessionContext.Country;
            this.City = this.sessionContext.City;
            this.StreetName = this.sessionContext.StreetName;
            this.StreetNumber = this.sessionContext.StreetNumber;

            this.OnPropertyChanged(nameof(ProfileImage));

            Guid currentAccountId = this.sessionContext.AccountId;
            ServiceResult<AccountProfileDataTransferObject> profileResult = await this.accountService.GetProfileAsync(currentAccountId);

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
            Guid currentAccountId = this.sessionContext.AccountId;

            AccountProfileDataTransferObject updateInformation = new AccountProfileDataTransferObject
            {
                DisplayName = this.DisplayName,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                Country = this.Country,
                City = this.City,
                StreetName = this.StreetName,
                StreetNumber = this.StreetNumber
            };

            ServiceResult<bool> updateResult = await this.accountService.UpdateProfileAsync(currentAccountId, updateInformation);

            if (updateResult.Success)
            {
                if (!string.IsNullOrEmpty(this.pendingAvatarPath))
                {
                    this.AvatarUrl = await this.accountService.UploadAvatarAsync(currentAccountId, this.pendingAvatarPath);
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
            await this.accountService.RemoveAvatarAsync(this.sessionContext.AccountId);
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

            ServiceResult<bool> passwordChangeResult = await this.accountService.ChangePasswordAsync(
                this.sessionContext.AccountId,
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

        public Microsoft.UI.Xaml.Media.ImageSource ProfileImage
        {
            get
            {
                if (string.IsNullOrEmpty(this.AvatarUrl))
                {
                    return null;
                }
                try
                {
                    return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(this.AvatarUrl));
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
