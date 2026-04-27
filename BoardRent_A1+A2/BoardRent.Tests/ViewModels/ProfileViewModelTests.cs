using BoardRent.DataTransferObjects;
using BoardRent.Domain;
using BoardRent.Services;
using BoardRent.Utils;
using BoardRent.ViewModels;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BoardRent.Tests.ViewModels
{
    public class ProfileViewModelTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IFilePickerService> _mockFilePickerService;
        private readonly ProfileViewModel _systemUnderTest;
        private readonly Guid _testUserId;

        public ProfileViewModelTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthService = new Mock<IAuthService>();
            _mockFilePickerService = new Mock<IFilePickerService>();

            _testUserId = Guid.NewGuid();
            var fakeUser = new User { Id = _testUserId, Username = "testuser", DisplayName = "Test User" };
            SessionContext.GetInstance().Populate(fakeUser, "Standard User");

            _systemUnderTest = new ProfileViewModel(
                _mockUserService.Object,
                _mockAuthService.Object,
                _mockFilePickerService.Object);
        }

        [Fact]
        public async Task LoadProfile_ValidData_PopulatesProperties()
        {
            // Arrange
            var profileData = new UserProfileDataTransferObject
            {
                Id = _testUserId,
                Username = "loaded_user",
                DisplayName = "Loaded Name",
                Email = "test@test.com"
            };

            _mockUserService.Setup(s => s.GetProfileAsync(_testUserId))
                            .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(profileData));

            // Act
            await _systemUnderTest.LoadProfile();

            // Assert
            Assert.Equal("loaded_user", _systemUnderTest.Username);
            Assert.Equal("Loaded Name", _systemUnderTest.DisplayName);
            Assert.Equal("test@test.com", _systemUnderTest.Email);
        }

        [Fact]
        public async Task SaveProfileCommand_InvalidData_SetsErrorProperties()
        {
            // Arrange
            _systemUnderTest.DisplayName = "A"; // Invalid length

            var failResult = ServiceResult<bool>.Fail("DisplayName|Display name must be between 2 and 50 characters long.");
            _mockUserService.Setup(s => s.UpdateProfileAsync(_testUserId, It.IsAny<UserProfileDataTransferObject>()))
                            .ReturnsAsync(failResult);

            // Act
            _systemUnderTest.SaveProfileCommand.Execute(null);

            // Wait for the async command to complete (simple workaround for ICommand)
            await Task.Delay(50);

            // Assert
            Assert.Equal("Display name must be between 2 and 50 characters long.", _systemUnderTest.DisplayNameError);
        }

        [Fact]
        public async Task SelectAvatarCommand_UserPicksFile_SetsAvatarUrlPreview()
        {
            // Arrange
            var fakePath = "C:\\poza_test.jpg";
            _mockFilePickerService.Setup(s => s.PickImageFileAsync()).ReturnsAsync(fakePath);

            // Act
            _systemUnderTest.SelectAvatarCommand.Execute(null);
            await Task.Delay(50); // Allow async relay command to finish

            // Assert
            Assert.Equal(fakePath, _systemUnderTest.AvatarUrl);
            // It should NOT call UploadAvatarAsync yet (only on save)
            _mockUserService.Verify(s => s.UploadAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmError()
        {
            // Arrange
            _systemUnderTest.NewPassword = "Password123!";
            _systemUnderTest.ConfirmPassword = "DifferentPassword123!";

            // Act
            _systemUnderTest.SaveNewPasswordCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            Assert.Equal("Passwords don't match", _systemUnderTest.ConfirmPasswordError);
            _mockUserService.Verify(s => s.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}