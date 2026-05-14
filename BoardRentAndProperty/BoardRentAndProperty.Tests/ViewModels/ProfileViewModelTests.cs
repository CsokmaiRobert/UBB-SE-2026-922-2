using System;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using CommunityToolkit.Mvvm.Input;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class ProfileViewModelTests
    {
        private FakeClientAccountService accountService = null!;
        private FakeClientAuthService authService = null!;
        private FakeFilePickerService filePickerService = null!;
        private FakeSessionContext sessionContext = null!;
        private ProfileViewModel systemUnderTest = null!;
        private Guid testAccountId;

        [SetUp]
        public void SetUp()
        {
            this.testAccountId = Guid.NewGuid();
            this.accountService = new FakeClientAccountService();
            this.authService = new FakeClientAuthService();
            this.filePickerService = new FakeFilePickerService();
            this.sessionContext = new FakeSessionContext
            {
                AccountId = this.testAccountId,
                Username = "testuser",
                DisplayName = "Test User",
                Email = "test@test.com",
                PhoneNumber = string.Empty,
                Country = string.Empty,
                City = string.Empty,
                StreetName = string.Empty,
                StreetNumber = string.Empty,
            };

            this.systemUnderTest = new ProfileViewModel(
                this.accountService,
                this.authService,
                this.filePickerService,
                this.sessionContext);
        }

        [Test]
        public async Task LoadProfileAsync_ValidData_PopulatesProperties()
        {
            var profileData = new AccountProfileDataTransferObject
            {
                Id = this.testAccountId,
                Username = "loaded_user",
                DisplayName = "Loaded Name",
                Email = "loaded@test.com",
            };

            this.accountService.ProfileResult =
                ServiceResult<AccountProfileDataTransferObject>.Ok(profileData);

            await this.systemUnderTest.LoadProfileAsync();

            Assert.That(this.systemUnderTest.Username, Is.EqualTo("loaded_user"));
            Assert.That(this.systemUnderTest.DisplayName, Is.EqualTo("Loaded Name"));
            Assert.That(this.systemUnderTest.Email, Is.EqualTo("loaded@test.com"));
        }

        [Test]
        public async Task SaveProfileCommand_InvalidData_SetsDisplayNameError()
        {
            this.systemUnderTest.DisplayName = "A";

            var failureResult = ServiceResult<bool>.Fail("DisplayName|Display name must be between 2 and 50 characters long.");
            this.accountService.UpdateProfileResult = failureResult;

            await ((IAsyncRelayCommand)this.systemUnderTest.SaveProfileCommand).ExecuteAsync(null);

            Assert.That(this.systemUnderTest.DisplayNameError, Is.EqualTo("Display name must be between 2 and 50 characters long."));
        }

        [Test]
        public async Task SelectAvatarCommand_UserPicksFile_SetsAvatarUrlPreview()
        {
            string fakePath = "C:\\test_avatar.jpg";
            this.filePickerService.SelectedPath = fakePath;

            await ((IAsyncRelayCommand)this.systemUnderTest.SelectAvatarCommand).ExecuteAsync(null);

            Assert.That(this.systemUnderTest.AvatarUrl, Is.EqualTo(fakePath));
            Assert.That(this.accountService.UploadAvatarCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmError()
        {
            this.systemUnderTest.NewPassword = "Password123!";
            this.systemUnderTest.ConfirmPassword = "DifferentPassword123!";

            await ((IAsyncRelayCommand)this.systemUnderTest.SaveNewPasswordCommand).ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
            Assert.That(this.accountService.ChangePasswordCallCount, Is.EqualTo(0));
        }
    }
}
