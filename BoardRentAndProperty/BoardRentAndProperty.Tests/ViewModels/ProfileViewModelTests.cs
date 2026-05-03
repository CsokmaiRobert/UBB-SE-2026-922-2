using System;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class ProfileViewModelTests
    {
        private Mock<IAccountService> accountServiceMock = null!;
        private Mock<IAuthService> authServiceMock = null!;
        private Mock<IFilePickerService> filePickerServiceMock = null!;
        private Mock<ISessionContext> sessionContextMock = null!;
        private ProfileViewModel systemUnderTest = null!;
        private Guid testAccountId;

        [SetUp]
        public void SetUp()
        {
            this.accountServiceMock = new Mock<IAccountService>();
            this.authServiceMock = new Mock<IAuthService>();
            this.filePickerServiceMock = new Mock<IFilePickerService>();
            this.sessionContextMock = new Mock<ISessionContext>();

            this.testAccountId = Guid.NewGuid();
            this.sessionContextMock.SetupGet(context => context.AccountId).Returns(this.testAccountId);
            this.sessionContextMock.SetupGet(context => context.Username).Returns("testuser");
            this.sessionContextMock.SetupGet(context => context.DisplayName).Returns("Test User");
            this.sessionContextMock.SetupGet(context => context.Email).Returns("test@test.com");
            this.sessionContextMock.SetupGet(context => context.PhoneNumber).Returns(string.Empty);
            this.sessionContextMock.SetupGet(context => context.Country).Returns(string.Empty);
            this.sessionContextMock.SetupGet(context => context.City).Returns(string.Empty);
            this.sessionContextMock.SetupGet(context => context.StreetName).Returns(string.Empty);
            this.sessionContextMock.SetupGet(context => context.StreetNumber).Returns(string.Empty);

            this.systemUnderTest = new ProfileViewModel(
                this.accountServiceMock.Object,
                this.authServiceMock.Object,
                this.filePickerServiceMock.Object,
                this.sessionContextMock.Object);
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

            this.accountServiceMock
                .Setup(service => service.GetProfileAsync(this.testAccountId))
                .ReturnsAsync(ServiceResult<AccountProfileDataTransferObject>.Ok(profileData));

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
            this.accountServiceMock
                .Setup(service => service.UpdateProfileAsync(this.testAccountId, It.IsAny<AccountProfileDataTransferObject>()))
                .ReturnsAsync(failureResult);

            await ((IAsyncRelayCommand)this.systemUnderTest.SaveProfileCommand).ExecuteAsync(null);

            Assert.That(this.systemUnderTest.DisplayNameError, Is.EqualTo("Display name must be between 2 and 50 characters long."));
        }

        [Test]
        public async Task SelectAvatarCommand_UserPicksFile_SetsAvatarUrlPreview()
        {
            string fakePath = "C:\\test_avatar.jpg";
            this.filePickerServiceMock
                .Setup(service => service.PickImageFileAsync())
                .ReturnsAsync(fakePath);

            await ((IAsyncRelayCommand)this.systemUnderTest.SelectAvatarCommand).ExecuteAsync(null);

            Assert.That(this.systemUnderTest.AvatarUrl, Is.EqualTo(fakePath));
            this.accountServiceMock.Verify(service => service.UploadAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmError()
        {
            this.systemUnderTest.NewPassword = "Password123!";
            this.systemUnderTest.ConfirmPassword = "DifferentPassword123!";

            await ((IAsyncRelayCommand)this.systemUnderTest.SaveNewPasswordCommand).ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
            this.accountServiceMock.Verify(service => service.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
